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
        public UserPageManager PageDataManager { get; }
        public _CourseListPartialModel CourseListModel { get; }

        public IndexModel(UserPageManager pageDataManager)
        {
            PageDataManager = pageDataManager;
            CourseListModel = new _CourseListPartialModel { Courses = PageDataManager.GetAllCourses() };
        }

        public void OnGet()
        {
        }
    }
}
