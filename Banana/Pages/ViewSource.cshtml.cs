﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banana.Data;
using Banana.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Banana.Pages
{
    public class ViewSourceModel : PageModel
    {
        public UserPage UserPage { get; set; }
        public UserCourse UserCourse { get; set; }
        public string PageTitle { get; set; }
        public _PageListPartialModel _PageListPartialModel { get; set; }

        private UserPageManager _pageManager { get; set; }

        public ViewSourceModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public IActionResult OnGet(int id)
        {
            UserPage = _pageManager.GetPage(id);
            if (UserPage == null)
                return NotFound();

            PageTitle = UserPage.HtmlTitle;
            UserCourse = _pageManager.GetCourse(UserPage.CourseId);
            _PageListPartialModel = new _PageListPartialModel(_pageManager, UserCourse, UserPage);

            return Page();
        }
    }
}