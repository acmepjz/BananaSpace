using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Banana.Areas.Identity.Data;
using Banana.Data;
using Banana.Text;

namespace Banana.Pages
{
    public class NewCourseModel : PageModel
    {
        private UserPageManager _pageManager;
        private UserManager<BananaUser> _userManager;

        public NewCourseModel(UserPageManager pageManager, UserManager<BananaUser> userManager)
        {
            _pageManager = pageManager;
            _userManager = userManager;
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
            if (User?.Identity?.IsAuthenticated != true)
                return Actions.RedirectToLoginPage("/create");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Unauthorized();

            if (ModelState.IsValid)
            {
                string userName = User.Identity.Name;

                // each user is allowed to create at most 100 courses
                if (_pageManager.GetCoursesByCreator(userName).Count() >= 100)
                    return Actions.Status418();

                string htmlTitle = Parser.ToHtml(Input.Title, null, ParserOptions.SingleLine, out var data);
                string email = (await _userManager.GetUserAsync(User)).Email;
                var course = _pageManager.NewCourse(User.Identity.Name, email, htmlTitle);

                // remove possible duplicate before adding main page
                var duplicate = _pageManager.GetPage(course.MainPageId, true);
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
                    IsDraft = false,
                    Title = course.Title,
                    HtmlTitle = HttpUtility.HtmlEncode(course.Title)
                };

                _pageManager.AddPage(page, 0);
                _pageManager.SaveChanges();
                return Actions.RedirectToUserPage();
            }

            // something is wrong; redisplay page
            return Page();
        }
    }
}