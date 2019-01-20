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

namespace Banana.Pages
{
    public class EditPageModel : PageModel
    {
        private readonly UserPageManager _userPageManager;

        public EditPageModel(UserPageManager userPageManager)
        {
            _userPageManager = userPageManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [MaxLength(100, ErrorMessage = "标题长度不能超过 100 字符。")]
            [Display(Name = "页面标题")]
            public string Title { get; set; }

            [MaxLength(500000, ErrorMessage = "内容长度不能超过 500 000 字符。")]
            [Display(Name = "页面内容")]
            [DataType(DataType.MultilineText)]
            public string Content { get; set; }

            [Display(Name = "章节编号")]
            public string SectionNumber { get; set; }
        }

        public readonly string ContentUIHint = "在这里输入页面内容。使用 $ ... $ 或 \\[ ... \\] 插入数学公式，使用 \\textbf 和 \\textit 等设置加粗和倾斜。你可以将自己定义的 LaTeX 命令放在开头，但不能使用 \\usepackage。\r\n\r\n编写新页面时，建议在本地编写好后复制到这里。";
        
        public UserPage UserPage { get; set; }
        public UserPage DraftPage { get; set; }
        public UserCourse UserCourse { get; set; }
        public IEnumerable<UserPage> AllPagesInCourse { get; set; }

        public void OnGet(string id = null)
        {
            if (int.TryParse(id, out int pageId))
            {
                UserPage = _userPageManager.GetPage(pageId);
                if (UserPage == null) return;

                ViewData["Title"] = "编辑: " + UserPage.Title;
                if (UserPage.DraftId != null)
                    DraftPage = _userPageManager.GetPage(UserPage.DraftId ?? 0);
                UserCourse = _userPageManager.GetCourse(UserPage.CourseId);
                AllPagesInCourse = _userPageManager.GetAllPages(UserPage.CourseId);

                var page = DraftPage ?? UserPage;
                Input = new InputModel
                {
                    Title = page.Title,
                    Content = page.Content,
                    SectionNumber = page.SectionNumber
                };
            }
        }

        private void UpdateContent(UserPage page)
        {
            string title = Input.Title;
            if (string.IsNullOrWhiteSpace(title)) title = "无标题";
            page.Title = title;
            page.HtmlTitle = Parser.ToHtml(title, page, ParserOptions.SingleLine);

            page.Content = Input.Content;
            page.HtmlContent = Parser.ToHtml(Input.Content, page, ParserOptions.Default);
        }

        public IActionResult OnPost(int id, bool publish = false)
        {
            if (ModelState.IsValid)
            {
                UserPage = _userPageManager.GetPage(id);
                if (UserPage == null)
                    return LocalRedirect("~/");
                if (UserPage.DraftId != null)
                    DraftPage = _userPageManager.GetPage(UserPage.DraftId ?? 0);

                if (publish) // user clicked 'Publish'
                {
                    _userPageManager.Update(UserPage);
                    UpdateContent(UserPage);
                    UserPage.DraftId = null;
                    UserPage.IsPublic = true;
                    UserPage.LastModifiedDate = DateTime.Now;

                    if (DraftPage != null)
                        _userPageManager.Remove(DraftPage);

                    _userPageManager.SaveChanges();
                    return LocalRedirect($"~/page/{id}");
                }

                // if we are here then user has clicked 'Preview'
                if (UserPage.IsPublic && DraftPage == null)
                {
                    int newId = _userPageManager.NewId();

                    var page = UserPage.CopyAsDraft();
                    page.Id = newId;
                    UpdateContent(page);
                    _userPageManager.Add(page);

                    _userPageManager.Update(UserPage);
                    UserPage.DraftId = newId;
                }
                else if (!UserPage.IsPublic)
                {
                    _userPageManager.Update(UserPage);
                    UpdateContent(UserPage);
                }
                else // IsPublic && DraftPage != null
                {
                    _userPageManager.Update(DraftPage);
                    UpdateContent(DraftPage);
                }

                _userPageManager.SaveChanges();
                return LocalRedirect($"~/page/{id}/edit");
            }

            // TODO: it should really be 'return Page();' but it doesn't work
            return LocalRedirect($"~/page/{id}/edit");
        }
    }
}