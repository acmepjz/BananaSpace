using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Banana.Data;

namespace Banana.Pages
{
    public class ViewPageModel : PageModel
    {
        public UserPage UserPage { get; set; }
        public UserCourse UserCourse { get; set; }
        public IEnumerable<UserPage> AllPagesInCourse { get; set; }

        private UserPageManager _userPageManager { get; set; }

        public ViewPageModel(UserPageManager userPageManager)
        {
            _userPageManager = userPageManager;
        }

        public void OnGet(string id = null)
        {
            if (int.TryParse(id, out int pageId))
            {
                UserPage = _userPageManager.GetPage(pageId);
                if (UserPage == null) return;

                UserCourse = _userPageManager.GetCourse(UserPage.CourseId);
                AllPagesInCourse = _userPageManager.GetAllPages(UserPage.CourseId);
            }
        }
    }
}