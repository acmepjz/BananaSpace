using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Banana.Data
{
    public class UserCourse
    {
        public int Id { get; set; }
        public string Creator { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int MainPageId { get; set; }
        public CourseStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int LastUpdatedPageId { get; set; }
    }

    public enum CourseStatus
    {
        Normal = 0,
        Request = 1,
        Deleted = 2
    }

    public class UserCoursePagesItem
    {
        public int CourseId { get; set; }
        public int PageId { get; set; }
        public int PageIndex { get; set; }
    }
}
