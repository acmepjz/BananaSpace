using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Banana.Data;
using Banana.Text;
using Microsoft.AspNetCore.Html;

namespace Banana.Pages
{
    public class EditPageModel : PageModel
    {
        private readonly UserPageManager _pageManager;

        public EditPageModel(UserPageManager userPageManager)
        {
            _pageManager = userPageManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [MaxLength(100, ErrorMessage = "标题长度不能超过 100 字符。")]
            [Display(Name = "页面标题")]
            public string Title { get; set; }

            [MaxLength(100000, ErrorMessage = "内容长度不能超过 100 000 字符。")]
            [Display(Name = "页面内容")]
            [DataType(DataType.MultilineText)]
            public string Content { get; set; }

            [Display(Name = "章节编号")]
            public string SectionNumber { get; set; }
        }

        public readonly string ContentUIHint = "在这里输入 TeX / LaTeX 代码...";
        
        public UserPage UserPage { get; set; }
        public UserPage DraftPage { get; set; }
        public UserCourse UserCourse { get; set; }
        public IEnumerable<UserPage> AllPagesInCourse { get; set; }
        public string PageTitle { get; set; }
        public string PageContent { get; set; }

        public void OnGet(string id = null)
        {
            if (int.TryParse(id, out int pageId))
            {
                UserPage = _pageManager.GetPage(pageId);
                if (UserPage == null) return;

                ViewData["Title"] = "编辑: " + UserPage.Title;
                if (UserPage.DraftId != null)
                    DraftPage = _pageManager.GetPage(UserPage.DraftId ?? 0);
                UserCourse = _pageManager.GetCourse(UserPage.CourseId);
                AllPagesInCourse = _pageManager.GetAllPages(UserPage.CourseId);

                var page = DraftPage ?? UserPage;
                PageTitle = page.Title;
                PageContent = page.GetFinalHtml(_pageManager, UserPage);
                Input = new InputModel
                {
                    Title = page.Title,
                    Content = page.Content,
                    SectionNumber = page.SectionNumber
                };
            }
        }

        private void UpdateContent(UserPage page, out ExpansionData data)
        {
            string title = Input.Title;
            if (string.IsNullOrWhiteSpace(title)) title = "无标题";
            page.Title = title;
            page.HtmlTitle = Parser.ToHtml(title, page, ParserOptions.SingleLine, out data);

            var labels = _pageManager.GetAllLabels(page.CourseId);
            page.Content = Input.Content;
            page.HtmlContent = Parser.ToHtml(Input.Content, page, ParserOptions.Default, out data);
        }

        private void UpdateLabels(UserPage page, ExpansionData data)
        {
            _pageManager.ClearLabels(page);

            foreach (var bookmark in data.Bookmarks)
            {
                string content = Parser.ToHtmlFinal(bookmark.Content, ParserOptions.SingleLine);
                _pageManager.AddLabel(page, bookmark.Label, content);
            }
        }

        // form data must contain an 'Action' field.
        //  - 'save':    responds with html in the form  <h1>...</h1><div>...</div>
        //  - 'publish': redirects to ViewPage.cshtml
        public IActionResult OnPost(int id)
        {
            var form = Request.Form;
            if (form == null) return BadRequest();
            bool publish = false;
            string action = form["Action"];
            if (action == "publish") publish = true;
            else if (action == "save") { }
            else return BadRequest();
            UserPage = _pageManager.GetPage(id);
            if (UserPage == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest();

            if (UserPage.DraftId != null)
                DraftPage = _pageManager.GetPage(UserPage.DraftId ?? 0);

            if (publish) // user clicked 'Publish'
            {
                var now = DateTime.Now;
                var course = _pageManager.GetCourse(UserPage.CourseId);
                _pageManager.Update(UserPage);
                UpdateContent(UserPage, out var data);
                UserPage.DraftId = null;
                UserPage.LastModifiedDate = now;

                if (DraftPage != null)
                    _pageManager.Remove(DraftPage);

                if (!UserPage.IsPublic) // first publishing
                {
                    UserPage.IsPublic = true;
                    UserPage.CreationDate = UserPage.LastModifiedDate;
                    _pageManager.Update(course);
                    course.LastUpdatedDate = now;
                    course.LastUpdatedPageId = UserPage.Id;
                }

                if (course.MainPageId == UserPage.Id)
                    course.Title = UserPage.HtmlTitle;

                UpdateLabels(UserPage, data);

                _pageManager.SaveChanges();
                return LocalRedirect($"~/page/{id}");
            }

            // if we are here then Action == 'save'
            UserPage page;
            if (UserPage.IsPublic && DraftPage == null)
            {
                int newId = _pageManager.NewId();

                page = UserPage.CopyAsDraft();
                page.Id = newId;
                UpdateContent(page, out var data);
                _pageManager.Add(page);

                _pageManager.Update(UserPage);
                UserPage.DraftId = newId;
            }
            else if (!UserPage.IsPublic)
            {
                page = UserPage;
                _pageManager.Update(page);
                UpdateContent(page, out var data);
            }
            else // IsPublic && DraftPage != null
            {
                page = DraftPage;
                _pageManager.Update(page);
                UpdateContent(page, out var data);
            }

            _pageManager.SaveChanges();

            string html = "<h1 class=\"box-h1\">" + (page.SectionNumber == null ? "" : page.SectionNumber + ". ") + page.HtmlTitle + "</h1>" +
                "<div class=\"user-page-content\">" + page.GetFinalHtml(_pageManager, UserPage) + "</div>";
            return Content(html);
        }
    }
}