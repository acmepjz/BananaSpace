using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banana.Data;
using Banana.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Banana.Pages
{
    public class UserProfileModel : PageModel
    {
        private UserPageManager _pageManager;

        public _CourseListPartialModel CourseListPartialModel { get; set; }

        public bool IsAdmin { get; set; }

        public UserProfileModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public IActionResult OnGet()
        {
            if (User?.Identity.IsAuthenticated != true)
                return Actions.RedirectToLoginPage();

            var userName = User.Identity.Name;
            CourseListPartialModel = new _CourseListPartialModel(_pageManager)
            {
                Courses = _pageManager.GetCoursesByFollower(userName)
            };

            IsAdmin = _pageManager.UserIsAdmin(userName);

            return Page();
        }
    }
}