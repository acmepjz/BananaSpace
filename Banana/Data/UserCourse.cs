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
        public int MainPageId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int LastUpdatedPageId { get; set; }
        public string Password { get; set; }
    }

    public class UserCoursePagesItem
    {
        public int CourseId { get; set; }
        public int PageId { get; set; }
        public int PageIndex { get; set; }
    }

    public class UserFavoritesItem
    {
        public string UserName { get; set; }
        public int CourseId { get; set; }
    }

    public class UserAccessItem
    {
        public string UserName { get; set; }
        public int CourseId { get; set; }
        public bool HasAccess { get; set; }
        public int AttemptCount { get; set; }
    }
}
