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
    public class IndexModel : PageModel
    {
        private UserPageManager _pageManager;
        public _CourseListPartialModel CourseListModel { get; }

        public IndexModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
            CourseListModel = new _CourseListPartialModel(pageManager) { Courses = _pageManager.GetAllCourses() };
        }

        public void OnGet()
        {
        }
    }
}
