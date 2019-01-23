using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Banana.Data;
using System.ComponentModel.DataAnnotations;
using Banana.Text;

namespace Banana.Pages
{
    public class ManageCourseModel : PageModel
    {
        public const int MaxPagesInCourse = 300;

        public UserCourse UserCourse { get; set; }
        public List<UserPage> UserPages { get; set; }
        public Dictionary<UserPage, UserPage> DraftPages { get; set; }

        private UserPageManager _pageManager;

        [BindProperty]
        public AddPageInputModel Input { get; set; }

        public class AddPageInputModel
        {
            [Display(Name = "章节编号")]
            public string SectionNumber { get; set; }

            [Display(Name = "页面标题")]
            public string Title { get; set; }
        }

        public ManageCourseModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public IActionResult OnGet(string id = null)
        {
            if (User?.Identity.IsAuthenticated == true &&
                int.TryParse(id, out int courseId))
            {
                UserCourse = _pageManager.GetCourse(courseId);
                if (UserCourse == null ||
                    !(User.Identity.Name == UserCourse.Creator || _pageManager.UserIsAdmin(User.Identity.Name)))
                    return Actions.RedirectTo404Page();

                UserPages = _pageManager.GetAllPages(courseId).ToList();
                DraftPages = new Dictionary<UserPage, UserPage>();
                foreach (var page in UserPages)
                    if (page.DraftId != null)
                        DraftPages.Add(page, _pageManager.GetPage((int)page.DraftId));

                ViewData["Title"] = "课程管理: " + UserCourse.Title;
                return Page();
            }

            return Actions.RedirectTo404Page();
        }

        private void PublishPage(UserPage page, DateTime timeStamp)
        {
            if (page.DraftId != null)
            {
                var draft = _pageManager.GetPage((int)page.DraftId);

                _pageManager.Update(page);
                draft.CopyTo(page);
                page.DraftId = null;
                page.LastModifiedDate = timeStamp;

                _pageManager.Remove(draft);
            }
            else if (!page.IsPublic)
            {
                _pageManager.Update(page);
                page.IsPublic = true;
                page.CreationDate = timeStamp;
                page.LastModifiedDate = timeStamp;
            }
        }

        public IActionResult OnPostPublish(int id)
        {
            if (User?.Identity.IsAuthenticated == true)
            {
                var page = _pageManager.GetPage(id);
                if (page == null)
                    return Actions.RedirectTo404Page();

                var course = _pageManager.GetCourse(page.CourseId);
                if (!(User.Identity.Name == course.Creator || _pageManager.UserIsAdmin(User.Identity.Name)))
                    return Actions.RedirectTo404Page();

                PublishPage(page, DateTime.Now);
                _pageManager.SaveChanges();

                return LocalRedirect($"~/page/{id}");
            }

            return Actions.RedirectTo404Page();
        }

        public IActionResult OnPostPublishAll(int id)
        {
            if (User?.Identity.IsAuthenticated == true)
            {
                var course = _pageManager.GetCourse(id);
                if (!(User.Identity.Name == course.Creator || _pageManager.UserIsAdmin(User.Identity.Name)))
                    return Actions.RedirectTo404Page();

                var pages = _pageManager.GetAllPages(id).ToList();
                var now = DateTime.Now;
                foreach (var page in pages)
                    PublishPage(page, now);
                _pageManager.SaveChanges();

                return LocalRedirect($"~/manage/{id}");
            }

            return Actions.RedirectTo404Page();
        }

        public IActionResult OnPostAddPage(int id, int option)
        {
            // option: 0 - insert above; 1 - insert below; 2 - insert as subpage

            if (User?.Identity.IsAuthenticated == true)
            {
                var page = _pageManager.GetPage(id);
                if (page == null)
                    return Actions.RedirectTo404Page();

                var course = _pageManager.GetCourse(page.CourseId);
                if (!(User.Identity.Name == course.Creator || _pageManager.UserIsAdmin(User.Identity.Name)))
                    return Actions.RedirectTo404Page();

                var pages = _pageManager.GetAllPages(course.Id).ToList();

                // validate
                if (page.Id == course.MainPageId && option != 1)
                    return Actions.RedirectTo404Page();
                if (page.PageLevel > 2 && option == 2)
                    return Actions.RedirectTo404Page();
                if (pages.Count >= MaxPagesInCourse)
                    return Actions.RedirectTo404Page();

                // create and add the page
                int index = pages.IndexOf(page),
                    level = page.PageLevel;

                if (option == 1 || option == 2)
                {
                    index++;
                    while (index < pages.Count && pages[index].PageLevel > level)
                        index++;
                    if (option == 2)
                        level++;
                }

                string sectionNumber = Input.SectionNumber?.Trim();
                if (sectionNumber != null && sectionNumber.Length > 50)
                    sectionNumber = sectionNumber.Substring(0, 50);
                if (string.IsNullOrWhiteSpace(sectionNumber))
                    sectionNumber = null;
                string theoremPrefix = sectionNumber;
                if (theoremPrefix != null)
                {
                    theoremPrefix = " " + theoremPrefix;
                    for (int i = theoremPrefix.Length - 1; i >= 0; i--)
                        if (char.IsWhiteSpace(theoremPrefix[i]))
                        {
                            theoremPrefix = theoremPrefix.Substring(i + 1);
                            break;
                        }
                }

                int newId = _pageManager.NewId();
                var now = DateTime.Now;
                string title = string.IsNullOrWhiteSpace(Input.Title) ? "无标题" : Input.Title;
                if (title.Length > 100)
                    title = sectionNumber.Substring(0, 100);

                var newPage = new UserPage
                {
                    CourseId = course.Id,
                    CreationDate = now,
                    Id = newId,
                    IsPublic = false,
                    LastModifiedDate = now,
                    PageLevel = level,
                    SectionNumber = sectionNumber,
                    TheoremPrefix = theoremPrefix,
                    Title = title
                };
                newPage.HtmlTitle = Parser.ToHtml(title, newPage, ParserOptions.SingleLine, out var data);
                _pageManager.AddPage(newPage, index);

                _pageManager.SaveChanges();
                return LocalRedirect($"~/manage/{course.Id}");
            }

            return Actions.RedirectTo404Page();
        }

        public IActionResult OnPostDeletePage(int id)
        {
            if (User?.Identity.IsAuthenticated == true)
            {
                var page = _pageManager.GetPage(id);
                if (page == null)
                    return Actions.RedirectTo404Page();

                var course = _pageManager.GetCourse(page.CourseId);
                if (!(User.Identity.Name == course.Creator || _pageManager.UserIsAdmin(User.Identity.Name)))
                    return Actions.RedirectTo404Page();

                if (page.Id == course.MainPageId)
                    return Actions.RedirectTo404Page();

                _pageManager.DeletePage(page);
                _pageManager.SaveChanges();
                return LocalRedirect($"~/manage/{course.Id}");
            }

            return Actions.RedirectTo404Page();
        }
    }
}