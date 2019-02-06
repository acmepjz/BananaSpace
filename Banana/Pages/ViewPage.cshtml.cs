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
        public bool IsCreatorOrAdmin { get; set; }
        public string PageTitle { get; set; }
        public string PageContent { get; set; }

        private UserPageManager _pageManager { get; set; }

        public ViewPageModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public IActionResult OnGet(string id = null)
        {
            if (int.TryParse(id, out int pageId))
            {
                UserPage = _pageManager.GetPage(pageId);
                if (UserPage == null)
                    return NotFound();

                PageTitle = UserPage.HtmlTitle;
                PageContent = UserPage.GetFinalHtml(_pageManager);
                UserCourse = _pageManager.GetCourse(UserPage.CourseId);
                AllPagesInCourse = _pageManager.GetAllPages(UserPage.CourseId);

                if (User?.Identity.IsAuthenticated == true)
                    IsCreatorOrAdmin = User.Identity.Name == UserCourse.Creator ||
                        _pageManager.UserIsAdmin(User.Identity.Name);
                return Page();
            }

            return NotFound();
        }
    }
}