﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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
        public string Files { get; set; }

        public UserPage() { }

        public string GetFinalHtml(UserPageManager pageManager)
        {
            string s = HtmlContent, html = "";

            int index;
            var labels = pageManager.GetAllLabels(CourseId);
            var files = GetFileList();

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
                        html += $"<img src=\"/uploads/{Id}/{hashedName}\" alt=\"{HttpUtility.HtmlEncode(name)}\" ";
                        if (style != null) html += $"style=\"{HttpUtility.HtmlEncode(style)}\" ";
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
                    var name = s.Substring(0, i);
                    s = s.Substring(i + 1);
                    
                    var l = (from label in labels where label.Key == name select label).FirstOrDefault();
                    if (l != null)
                    {
                        html += $"<a href=\"/page/{l.PageId}#{l.Key}\">{l.Content}</a>";
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
            page.TheoremPrefix = TheoremPrefix;
            page.PageLevel = PageLevel;
            page.Preamble = Preamble;
            page.Content = Content;
            page.HtmlContent = HtmlContent;
            page.Files = Files;
        }
    }
}
