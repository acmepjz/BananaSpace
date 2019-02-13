using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Banana.Data
{
    public class UserPage
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public bool IsPublic { get; set; }
        public string Title { get; set; } = "";
        public string HtmlTitle { get; set; } = "";
        public string SectionNumber { get; set; }
        public int PageLevel { get; set; }
        public string Content { get; set; } = "";
        public string HtmlContent { get; set; } = "";
        public int? DraftId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Files { get; set; }

        public UserPage() { }

        const string ImageStyleRegex = @"^((width|height)=\s*[0-9]+(.[0-9]+)?(%|px|pt|em|ex)(,\s*|$))+$";

        public string GetFinalHtml(UserPageManager pageManager, UserPage nonDraftPage)
        {
            if (HtmlContent == null) return "";
            string s = HtmlContent, html = "";

            int index;
            var labels = pageManager.GetAllLabels(CourseId);
            var files = nonDraftPage.GetFileList();

            while (s.Contains("<:"))
            {
                index = s.IndexOf("<:");
                html += s.Substring(0, index);
                s = s.Substring(index);

                if (s.StartsWith("<:FILE:"))
                {
                    s = s.Substring(7);
                    int i = s.IndexOf('>');
                    if (i == -1) throw new Exception();
                    var name = HttpUtility.HtmlDecode(s.Substring(0, i));
                    s = s.Substring(i + 1);

                    i = name.IndexOf(':');
                    string style = null;
                    if (i != -1)
                    {
                        style = name.Substring(i + 1);
                        name = name.Substring(0, i);
                    }

                    if (files.TryGetValue(name, out string hashedName))
                    {
                        html += $"<img src=\"/uploads/{nonDraftPage.Id}/{hashedName}\" alt=\"{HttpUtility.HtmlEncode(name)}\" ";
                        if (style != null && Regex.IsMatch(style, ImageStyleRegex))
                        {
                            style = style.Replace('=', ':').Replace(',', ';');
                            html += $"style=\"{style}\" ";
                        }
                        html += "/>";
                    }
                    else
                    {
                        html += $"<span style=\"color:red;font-weight:bold\">{HttpUtility.HtmlEncode(name)}</span>";
                    }
                }
                else if (s.StartsWith("<:REF:"))
                {
                    s = s.Substring(6);
                    int i = s.IndexOf('>');
                    if (i == -1) throw new Exception();
                    string _ref = s.Substring(0, i), text = null;
                    s = s.Substring(i + 1);
                    if (_ref.Contains(":TEXT:"))
                    {
                        int ii = _ref.IndexOf(":TEXT:");
                        text = _ref.Substring(ii + 6);
                        _ref = _ref.Substring(0, ii);
                    }

                    var l = (from label in labels
                             where label.Key == _ref
                             select label).FirstOrDefault();
                    if (l != null)
                    {
                        html += $"<a href=\"/page/{l.PageId}#{l.Key}\">{text ?? l.Content}</a>";
                        break;
                    }

                    html += "<span style=\"color:red;font-weight:bold\">??</span>";
                }
            }

            html += s;
            return html;
        }

        public Dictionary<string, string> GetFileList()
        {
            string s = Files + ";";
            var dict = new Dictionary<string, string>();
            while (s.Length > 1)
            {
                int i = s.IndexOf(';'), j = s.IndexOf(':');
                if (j > i)
                    throw new FormatException();
                dict.Add(s.Substring(0, j), s.Substring(j + 1, i - j - 1));
                s = s.Substring(i + 1);
            }
            return dict;
        }

        public void SaveFilesString(Dictionary<string, string> files)
        {
            var list = files.ToList();
            list.Sort((l, r) => l.Key.CompareTo(r.Key));
            string s = "";
            foreach (var v in list)
                s += v.Key + ":" + v.Value + ";";
            Files = s;
        }

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
            page.PageLevel = PageLevel;
            page.Content = Content;
            page.HtmlContent = HtmlContent;
            page.Files = Files;
        }
    }
}
