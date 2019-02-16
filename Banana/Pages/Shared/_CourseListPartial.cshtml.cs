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

        private UserPageManager _pageManager;
        public _CourseListPartialModel(UserPageManager pageManager) { _pageManager = pageManager; }

        public string DateTimeString(DateTime dateTime, bool insertSpace)
        {
            var now = DateTime.Now;
            int days = (int)(now.Date - dateTime.Date).TotalDays;
            return days == 0 ? "今天" :
                days == 1 ? "昨天" :
                days == 2 ? "前天" :
                days > 0 && days <= 7 ? $"{days} 天前" :
                dateTime.ToString("yyyy-MM-dd") + (insertSpace ? " " : "");
        }

        public UserPage GetNewestPage(UserCourse course)
        {
            return _pageManager.GetPage(course.LastUpdatedPageId, false);
        }

        public void OnGet()
        {
        }
    }
}