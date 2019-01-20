using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Banana.Data
{
    public class UserPage
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public bool IsPublic { get; set; }
        public string Title { get; set; }
        public string HtmlTitle { get; set; }
        public string SectionNumber { get; set; }
        public string TheoremPrefix { get; set; }
        public int PageLevel { get; set; }
        public string Preamble { get; set; }
        public string Content { get; set; }
        public string HtmlContent { get; set; }
        public int? DraftId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public UserPage() { }

        public UserPage CopyAsDraft()
        {
            var copy = new UserPage
            {
                Id = Id,
                CourseId = CourseId,
                IsPublic = false,
                CreationDate = CreationDate,
                LastModifiedDate = DateTime.Now
            };
            CopyTo(copy);
            return copy;
        }

        public void CopyTo(UserPage page)
        {
            page.Title = Title;
            page.HtmlTitle = HtmlTitle;
            page.SectionNumber = SectionNumber;
            page.TheoremPrefix = TheoremPrefix;
            page.PageLevel = PageLevel;
            page.Preamble = Preamble;
            page.Content = Content;
            page.HtmlContent = HtmlContent;
        }
    }
}
