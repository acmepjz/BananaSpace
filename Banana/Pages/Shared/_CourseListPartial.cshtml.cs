using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Banana.Data;

namespace Banana.Pages.Shared
{
    public class _CourseListPartialModel : PageModel
    {
        public IEnumerable<UserCourse> Courses { get; set; }
        public CourseListType Type { get; set; } = CourseListType.View;

        public void OnGet()
        {
        }
    }

    public enum CourseListType
    {
        View,
        ManageRequests
    }
}