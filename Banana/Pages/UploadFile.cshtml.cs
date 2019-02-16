using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Banana.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Banana.Pages
{
    public class UploadFileModel : PageModel
    {
        private UserPageManager _pageManager;

        private readonly string[] AllowedExtensions =
        {
            ".jpeg", ".jpg", ".png", ".svg"
        };

        public UploadFileModel(UserPageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public IActionResult OnGet()
        {
            return Actions.Status405();
        }

        public IActionResult OnPost()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Unauthorized();
            var form = Request.Form;
            if (form == null)
                return BadRequest();
            var pageId = form["PageId"];
            if (!int.TryParse(pageId, out int id))
                return BadRequest();
            var page = _pageManager.GetPage(id, false);
            if (page == null)
                return BadRequest();
            if (!_pageManager.UserCanEdit(User.Identity.Name, page.CourseId))
                return Forbid();

            var action = form["Action"];
            switch (action)
            {
                case "upload":
                    if (form.Files == null || form.Files.Count != 1)
                        return BadRequest();

                    var file = form.Files.Single();
                    if (!AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                        return BadRequest();
                    if (file.Length >= 2 << 20) // 2 MB
                        return BadRequest();

                    if (_pageManager.AddFile(page, file))
                        return Page();
                    return Actions.Status500();

                case "delete":
                    string fileName = form["FileName"];
                    if (fileName == null)
                        return BadRequest();

                    var result = _pageManager.DeleteFile(page, fileName);
                    if (result == false)
                        return BadRequest();
                    if (result == null)
                        return Actions.Status500();

                    return Page();

                default:
                    return BadRequest();
            }
        }
    }
}