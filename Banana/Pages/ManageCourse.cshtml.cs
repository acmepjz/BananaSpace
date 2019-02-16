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

        [BindProperty]
        [Display(Name = "设置密码")]
        [MaxLength(64)]
        public string Password { get; set; }

        public ManageCourseModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public IActionResult OnGet(int id = -1)
        {
            if (id == -1) return NotFound();

            if (User?.Identity?.IsAuthenticated != true)
                return Actions.RedirectToLoginPage($"/manage/{id}");
            
            UserCourse = _pageManager.GetCourse(id);
            if (UserCourse == null)
                return NotFound();
            if (!_pageManager.UserCanEdit(User.Identity.Name, id))
                return Forbid();

            UserPages = _pageManager.GetAllPages(id).ToList();
            DraftPages = new Dictionary<UserPage, UserPage>();
            foreach (var page in UserPages)
                if (page.DraftId != null)
                    DraftPages.Add(page, _pageManager.GetPage((int)page.DraftId, true));

            Password = UserCourse.Password;

            ViewData["Title"] = "管理文档";
            return Page();
        }

        private void PublishPage(UserPage page)
        {
            if (page.DraftId != null)
            {
                var draft = _pageManager.GetPage((int)page.DraftId, true);
                draft.CopyTo(page);
                Parser.ToHtml(draft.Content, page, ParserOptions.Default, out var data);
                _pageManager.PublishPage(page, data);
            }
            else if (!page.IsPublic)
            {
                // first publishing
                Parser.ToHtml(page.Content, page, ParserOptions.Default, out var data);
                _pageManager.PublishPage(page, data);
            }
        }

        public IActionResult OnPost(int id = -1)
        {
            if (id == -1) return NotFound();
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized();
            var form = Request.Form;
            if (form == null) return BadRequest();
            string action = form["Action"];
            if (action == null) return BadRequest();
            if (!int.TryParse(form["PageId"], out int pageId)) return BadRequest();
            var page = _pageManager.GetPage(pageId, true);
            if (page == null || page.CourseId != id) return BadRequest();
            if (!_pageManager.UserCanEdit(User.Identity.Name, id)) return Forbid();
            var pages = _pageManager.GetAllPages(id).ToList();
            if (!pages.Contains(page)) return BadRequest();
            var now = DateTime.Now;
            var course = _pageManager.GetCourse(id);

            switch (action)
            {
                case "publish":
                    PublishPage(page);
                    _pageManager.SaveChanges();
                    return LocalRedirect($"~/page/{pageId}");

                //case "publish-all":
                //    foreach (var p in pages)
                //        PublishPage(p, course, now);
                //    _pageManager.SaveChanges();
                //    return LocalRedirect($"~/manage/{id}");

                case "insert-above":
                case "insert-below":
                case "insert-subpage":
                    // validate
                    if (page.Id == course.MainPageId && action != "insert-below")
                        return BadRequest();
                    if (page.PageLevel > 2 && action == "insert-subpage")
                        return BadRequest();
                    if (pages.Count >= MaxPagesInCourse)
                        return BadRequest();

                    // create and add the page
                    int index = pages.IndexOf(page),
                        level = page.PageLevel;
                    if (index == -1) return BadRequest();

                    if (action != "insert-above")
                    {
                        index++;
                        while (index < pages.Count && pages[index].PageLevel > level)
                            index++;
                        if (action == "insert-subpage")
                            level++;
                    }

                    string sectionNumber = Input.SectionNumber?.Trim();
                    if (sectionNumber != null && sectionNumber.Length > 50)
                        return BadRequest();
                    if (string.IsNullOrWhiteSpace(sectionNumber))
                        sectionNumber = null;

                    string title = string.IsNullOrWhiteSpace(Input.Title) ? "无标题" : Input.Title;
                    if (title.Length > 100)
                        return BadRequest();

                    int newId = _pageManager.NewId();
                    var newPage = new UserPage
                    {
                        CourseId = course.Id,
                        CreationDate = now,
                        Id = newId,
                        IsDraft = false,
                        IsPublic = false,
                        LastModifiedDate = now,
                        PageLevel = level,
                        SectionNumber = sectionNumber,
                        Title = title
                    };
                    newPage.HtmlTitle = Parser.ToHtml(title, newPage, ParserOptions.SingleLine, out var data);
                    _pageManager.AddPage(newPage, index);

                    _pageManager.SaveChanges();
                    return LocalRedirect($"~/manage/{course.Id}");

                case "delete":
                    if (page.Id == course.MainPageId)
                        return BadRequest();

                    _pageManager.DeletePage(page);
                    _pageManager.SaveChanges();
                    return LocalRedirect($"~/manage/{course.Id}");

                case "delete-draft":
                    if (page.DraftId == null)
                        return BadRequest();

                    _pageManager.DeleteDraftPage(page.DraftId ?? -1);
                    _pageManager.Update(page);
                    page.DraftId = null;
                    _pageManager.SaveChanges();
                    return LocalRedirect($"~/manage/{course.Id}");

                case "delete-all":
                    _pageManager.DeleteCourse(id);
                    _pageManager.SaveChanges();
                    return Actions.RedirectToUserPage();

                case "set-password":
                    if (Password != null && Password.Length > 64)
                        return BadRequest();

                    var oldPassword = course.Password;
                    var newPassword = string.IsNullOrWhiteSpace(Password) ? null : Password.Trim();
                    if (oldPassword != newPassword)
                    {
                        _pageManager.Update(course);
                        course.Password = newPassword;
                        _pageManager.ResetAccess(course.Id);
                        _pageManager.SaveChanges();
                    }
                    return LocalRedirect($"~/manage/{course.Id}");

                default:
                    return BadRequest();
            }
        }
    }
}