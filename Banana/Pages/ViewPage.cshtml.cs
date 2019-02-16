using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Banana.Areas.Identity.Data;
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

        public IActionResult OnGet(int id = -1)
        {
            if (id == -1) return NotFound();

            UserPage = _pageManager.GetPage(id, false);
            if (UserPage == null || !UserPage.IsPublic)
                return NotFound();

            UserCourse = _pageManager.GetCourse(UserPage.CourseId);
            if (UserCourse.Password != null)
            {
                if (User?.Identity?.IsAuthenticated != true)
                    return Actions.RedirectToLoginPage($"/page/{id}");
                if (!_pageManager.HasAccess(User.Identity.Name, UserCourse))
                    return LocalRedirect($"~/page/{id}/verify");
            }

            PageTitle = UserPage.HtmlTitle;
            PageContent = UserPage.GetFinalHtml(_pageManager, UserPage);
            _PageListPartialModel = new _PageListPartialModel(_pageManager, UserCourse, UserPage);

            if (User?.Identity?.IsAuthenticated == true)
            {
                IsCreatorOrAdmin = User.Identity.Name == UserCourse.Creator ||
                    _pageManager.UserIsAdmin(User.Identity.Name);
                IsFavorite = _pageManager.UserIsFollower(User.Identity.Name, UserPage.CourseId);
            }
            return Page();
        }

        // adding/cancelling favorites is done by posting to this page (via ajax)
        public IActionResult OnPost(int id = -1)
        {
            if (id == -1) return NotFound();
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized();
            var form = Request.Form;
            if (form == null) return BadRequest();
            bool fav = false;
            string action = form["Action"];
            if (action == "add-fav") fav = true;
            else if (action == "cancel-fav") fav = false;
            else return BadRequest();
            UserPage = _pageManager.GetPage(id, false);
            if (UserPage == null || !UserPage.IsPublic) return NotFound();
            
            string userName = User.Identity.Name;
            if (fav) _pageManager.AddToFavorites(userName, UserPage.CourseId);
            else _pageManager.CancelFavorites(userName, UserPage.CourseId);
            _pageManager.SaveChanges();

            return Actions.Status204();
        }
    }
}