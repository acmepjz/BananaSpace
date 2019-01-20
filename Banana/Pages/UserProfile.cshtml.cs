using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banana.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Banana.Pages
{
    public class UserProfileModel : PageModel
    {
        private UserPageManager _pageManager;

        public IEnumerable<UserCourse> CreatedCourses { get; set; }
        public IEnumerable<UserCourse> FollowedCourses { get; set; }

        public bool IsAdmin { get; set; }
        public IEnumerable<UserCourse> AllCourseRequests { get; set; }

        public UserProfileModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public IActionResult OnGet()
        {
            if (User?.Identity.IsAuthenticated != true)
                return Actions.RedirectToLoginPage();

            var userName = User.Identity.Name;
            CreatedCourses = _pageManager.GetCoursesByCreator(userName);
            FollowedCourses = _pageManager.GetCoursesByFollower(userName);

            IsAdmin = _pageManager.UserIsAdmin(userName);
            if (IsAdmin)
            {
                AllCourseRequests = _pageManager.GetAllCourseRequests();
            }

            return Page();
        }
    }
}