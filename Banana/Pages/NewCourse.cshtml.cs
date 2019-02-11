using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Banana.Data;
using System.Web;
using Banana.Text;

namespace Banana.Pages
{
    public class NewCourseModel : PageModel
    {
        private UserPageManager _pageManager;

        public NewCourseModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public bool IsAdmin { get; set; } = false;
        public int? RequestId { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "{0}不能为空。")]
            [MaxLength(100, ErrorMessage = "标题长度不能超过 100 字符。")]
            [Display(Name = "文档标题")]
            public string Title { get; set; }
        }

        public IActionResult OnGet()
        {
            if (User?.Identity.IsAuthenticated != true)
                return Actions.RedirectToLoginPage();

            return Page();
        }

        public IActionResult OnPost()
        {
            if (User?.Identity.IsAuthenticated != true)
                return Actions.RedirectToLoginPage();

            if (ModelState.IsValid)
            {
                string userName = User.Identity.Name;

                // each user is allowed to create at most 100 courses
                if (_pageManager.GetCoursesByCreator(userName).Count() >= 100)
                    return Actions.Status418();

                var course = _pageManager.NewCourse(User.Identity.Name);

                // remove possible duplicate before adding main page
                var duplicate = _pageManager.GetPage(course.MainPageId);
                if (duplicate != null)
                    _pageManager.Remove(duplicate);

                // create main page of the course
                var now = DateTime.Now;
                var page = new UserPage
                {
                    Id = course.MainPageId,
                    CourseId = course.MainPageId,
                    CreationDate = now,
                    LastModifiedDate = now,
                    IsPublic = true,
                    Title = course.Title,
                    HtmlTitle = HttpUtility.HtmlEncode(course.Title)
                };

                string htmlTitle = Parser.ToHtml(Input.Title, page, ParserOptions.SingleLine, out var data);

                _pageManager.AddPage(page, 0);
                _pageManager.Update(course);
                course.Title = htmlTitle;
                _pageManager.SaveChanges();
                return Actions.RedirectToUserPage();
            }

            return Page();
        }
    }
}