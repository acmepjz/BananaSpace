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
            [MaxLength(100)]
            [Display(Name = "页面标题")]
            public string Title { get; set; }

            [Display(Name = "页面内容")]
            [DataType(DataType.MultilineText)]
            public string Content { get; set; }

            [MaxLength(50)]
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

        public IActionResult OnGet(int id = -1)
        {
            if (id == -1) return NotFound();

            UserPage = _pageManager.GetPage(id, false);
            if (UserPage == null) return NotFound();

            if (User?.Identity?.IsAuthenticated != true)
                return Actions.RedirectToLoginPage($"/page/{id}/edit");

            UserCourse = _pageManager.GetCourse(UserPage.CourseId);
            if (!_pageManager.UserCanEdit(User.Identity.Name, UserPage.CourseId))
                return Forbid();

            ViewData["Title"] = "编辑: " + UserPage.Title;
            if (UserPage.DraftId != null)
                DraftPage = _pageManager.GetPage(UserPage.DraftId ?? 0, true);
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
            return Page();
        }

        private void UpdateContent(UserPage page, out ExpansionData data)
        {
            page.SectionNumber = string.IsNullOrWhiteSpace(Input.SectionNumber) ? null : Input.SectionNumber.Trim();

            string title = Input.Title;
            if (string.IsNullOrWhiteSpace(title)) title = "无标题";
            page.Title = title;
            page.HtmlTitle = Parser.ToHtml(title, page, ParserOptions.SingleLine, out data);

            var labels = _pageManager.GetAllLabels(page.CourseId);
            if (Input.Content == null)
                Input.Content = "";
            if (Input.Content.Length <= Expression.MaxLength)
                page.Content = Input.Content;
            page.HtmlContent = Parser.ToHtml(Input.Content, page, ParserOptions.Default, out data);
        }

        // form data must contain an 'Action' field.
        //  - 'save':    responds with html in the form  <h1>...</h1><div>...</div>
        //  - 'publish': redirects to ViewPage.cshtml
        public IActionResult OnPost(int id = -1)
        {
            if (id == -1) return NotFound();
            UserPage = _pageManager.GetPage(id, false);
            if (UserPage == null) return NotFound();
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized();
            if (!_pageManager.UserCanEdit(User.Identity.Name, UserPage.CourseId)) return Forbid();
            var form = Request.Form;
            if (form == null) return BadRequest();
            bool publish = false;
            string action = form["Action"];
            if (action == "publish") publish = true;
            else if (action == "save") { }
            else return BadRequest();
            if (!ModelState.IsValid) return BadRequest();

            if (publish) // user clicked 'Publish'
            {
                UpdateContent(UserPage, out var data);
                _pageManager.PublishPage(UserPage, data);
                _pageManager.SaveChanges();
                return LocalRedirect($"~/page/{id}");
            }

            // if we are here then Action == 'save'
            if (UserPage.DraftId != null)
                DraftPage = _pageManager.GetPage(UserPage.DraftId ?? 0, true);
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