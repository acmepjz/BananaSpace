using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banana.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Banana.Pages
{
    public class VerifyModel : PageModel
    {
        private UserPageManager _pageManager;
        public VerifyModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public bool IsSecondAttempt { get; set; }

        [BindProperty]
        [Display(Name = "输入密码")]
        [MaxLength(64)]
        public string Password { get; set; }

        public IActionResult OnGet(int id = -1)
        {
            if (id == -1) return NotFound();

            var page = _pageManager.GetPage(id, false);
            if (page == null)
                return NotFound();

            var course = _pageManager.GetCourse(page.CourseId);
            if (User?.Identity?.IsAuthenticated != true)
                return Actions.RedirectToLoginPage($"/page/{id}");
            if (course.Password == null)
                return LocalRedirect($"~/page/{id}");
            if (_pageManager.IsBlocked(User.Identity.Name, page.CourseId))
                return Forbid();
            if (_pageManager.HasAccess(User.Identity.Name, course))
                return LocalRedirect($"~/page/{id}");

            return Page();
        }

        public IActionResult OnPost(int id = -1)
        {
            if (id == -1) return NotFound();
            if (User?.Identity?.IsAuthenticated != true)
                return Unauthorized();

            var page = _pageManager.GetPage(id, false);
            if (page == null)
                return NotFound();
            if (_pageManager.IsBlocked(User.Identity.Name, page.CourseId))
                return Forbid();

            var course = _pageManager.GetCourse(page.CourseId);
            if (course.Password == Password)
            {
                _pageManager.AddAccess(User.Identity.Name, course.Id);
                _pageManager.SaveChanges();
                return LocalRedirect($"~/page/{id}");
            }
            else
            {
                _pageManager.WrongPasswordAtAccess(User.Identity.Name, course.Id);
                _pageManager.SaveChanges();

                if (_pageManager.IsBlocked(User.Identity.Name, course.Id))
                    return Forbid();

                IsSecondAttempt = true;
                return Page();
            }
        }
    }
}