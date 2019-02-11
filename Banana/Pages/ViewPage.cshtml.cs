using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Banana.Data;
using Banana.Pages.Shared;

namespace Banana.Pages
{
    public class ViewPageModel : PageModel
    {
        public UserPage UserPage { get; set; }
        public UserCourse UserCourse { get; set; }
        public bool IsCreatorOrAdmin { get; set; }
        public bool IsFavorite { get; set; }
        public string PageTitle { get; set; }
        public string PageContent { get; set; }

        public _PageListPartialModel _PageListPartialModel { get; set; }

        private UserPageManager _pageManager { get; set; }

        public ViewPageModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public IActionResult OnGet(int id)
        {
            UserPage = _pageManager.GetPage(id);
            if (UserPage == null)
                return NotFound();

            PageTitle = UserPage.HtmlTitle;
            PageContent = UserPage.GetFinalHtml(_pageManager, UserPage);
            UserCourse = _pageManager.GetCourse(UserPage.CourseId);
            _PageListPartialModel = new _PageListPartialModel(_pageManager, UserCourse, UserPage);

            if (User?.Identity.IsAuthenticated == true)
            {
                IsCreatorOrAdmin = User.Identity.Name == UserCourse.Creator ||
                    _pageManager.UserIsAdmin(User.Identity.Name);
                IsFavorite = _pageManager.UserIsFollower(User.Identity.Name, UserPage.CourseId);
            }
            return Page();
        }

        // adding/cancelling favorites is done by posting to this page (via ajax)
        public IActionResult OnPost(int id)
        {
            if (User?.Identity.IsAuthenticated != true) return BadRequest();
            var form = Request.Form;
            if (form == null) return BadRequest();
            bool fav = false;
            string action = form["Action"];
            if (action == "add-fav") fav = true;
            else if (action == "cancel-fav") fav = false;
            else return BadRequest();
            UserPage = _pageManager.GetPage(id);
            if (UserPage == null) return BadRequest();
            
            string userName = User.Identity.Name;
            if (fav) _pageManager.AddToFavorites(userName, UserPage.CourseId);
            else _pageManager.CancelFavorites(userName, UserPage.CourseId);
            _pageManager.SaveChanges();

            return Actions.Status200();
        }
    }
}