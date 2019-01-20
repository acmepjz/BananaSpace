using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Banana.Data;

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
            [MaxLength(100, ErrorMessage = "课程标题长度不能超过 100 字符。")]
            [Display(Name = "课程标题")]
            public string Title { get; set; }

            [Required(ErrorMessage = "{0}不能为空。")]
            [MaxLength(50, ErrorMessage = "课程类型长度不能超过 50 字符。")]
            [Display(Name = "课程类型")]
            public string Type { get; set; }
        }

        public IActionResult OnGet(string requestId = null)
        {
            if (User?.Identity.IsAuthenticated != true)
                return Actions.RedirectToLoginPage();

            if (requestId != null && _pageManager.UserIsAdmin(User.Identity.Name) &&
                int.TryParse(requestId, out int id))
            {
                var course = _pageManager.GetCourse(id);
                if (course == null || course.Status != CourseStatus.Request)
                    return Actions.RedirectTo404Page();

                IsAdmin = true;
                RequestId = id;
                Input = new InputModel
                {
                    Title = course.Title,
                    Type = course.Type
                };
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (User?.Identity.IsAuthenticated != true)
                return Actions.RedirectToLoginPage();

            if (ModelState.IsValid)
            {
                _pageManager.NewCourseRequest(Input.Title, Input.Type, User.Identity.Name);
                return Actions.RedirectToUserPage();
            }

            return Page();
        }

        public IActionResult OnPostConfirm(int requestId)
        {
            if (User?.Identity.IsAuthenticated != true)
                return Actions.RedirectToLoginPage();
            if (!_pageManager.UserIsAdmin(User.Identity.Name))
                return Actions.RedirectTo404Page();

            if (ModelState.IsValid)
            {
                var course = _pageManager.GetCourse(requestId);
                if (course == null || course.Status != CourseStatus.Request)
                    return Actions.RedirectTo404Page();

                _pageManager.Update(course);
                course.Title = Input.Title;
                course.Type = Input.Type;
                course.Status = CourseStatus.Normal;
                course.CreationDate = DateTime.Now;
                course.LastUpdatedDate = DateTime.Now;
                course.LastUpdatedPageId = course.MainPageId;

                // remove possible duplicate before adding main page
                var duplicate = _pageManager.GetPage(course.MainPageId);
                if (duplicate != null)
                    _pageManager.Remove(duplicate);

                // create main page of the course
                var page = new UserPage
                {
                    Id = course.MainPageId,
                    CourseId = course.MainPageId,
                    CreationDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    IsPublic = true,
                    Title = course.Title,
                    HtmlTitle = course.Title
                };
                _pageManager.AddPage(page, 0);

                _pageManager.SaveChanges();
                return Actions.RedirectToUserPage();
            }

            return Page();
        }

        public IActionResult OnPostReject(int requestId)
        {
            if (User?.Identity.IsAuthenticated != true)
                return Actions.RedirectToLoginPage();
            if (!_pageManager.UserIsAdmin(User.Identity.Name))
                return Actions.RedirectTo404Page();

            var course = _pageManager.GetCourse(requestId);
            if (course == null || course.Status != CourseStatus.Request)
                return Actions.RedirectTo404Page();

            _pageManager.Remove(course);

            _pageManager.SaveChanges();
            return Actions.RedirectToUserPage();
        }
    }
}