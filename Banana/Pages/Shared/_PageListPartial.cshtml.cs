using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banana.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Banana.Pages.Shared
{
    public class _PageListPartialModel : PageModel
    {
        public UserCourse UserCourse { get; set; }
        public UserPage UserPage { get; set; }
        public IEnumerable<UserPage> AllPagesInCourse { get; set; }

        public _PageListPartialModel(UserPageManager pageManager, UserCourse course, UserPage page)
        {
            UserCourse = course;
            UserPage = page;
            AllPagesInCourse = pageManager.GetAllPages(course.Id, p => p.IsPublic);
        }

        public void OnGet()
        {

        }
    }
}