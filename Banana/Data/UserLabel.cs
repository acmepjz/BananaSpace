using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Banana.Data
{
    public class UserLabel
    {
        public string Id { get; set; }
        public int CourseId { get; set; }
        public int PageId { get; set; }
        public string Key { get; set; }
        public string Content { get; set; }
    }
}
