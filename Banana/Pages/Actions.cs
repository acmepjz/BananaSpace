using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Banana.Pages
{
    public static class Actions
    {
        public const string TemporaryAddress = "140.143.164.26";

        public static IActionResult RedirectToLoginPage(string returnUrl) 
            => new LocalRedirectResult("~/login" + (returnUrl == null ? "" : "?returnUrl=" + HttpUtility.UrlEncode(returnUrl)));

        public static IActionResult RedirectToUserPage()
            => new LocalRedirectResult("~/user");

        public static IActionResult Status204()
            => new NoContentResult();

        public static IActionResult Status405()
            => new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);

        public static IActionResult Status418()
            => new StatusCodeResult(StatusCodes.Status418ImATeapot); // 418 is for error msgs not yet implemented.

        public static IActionResult Status500()
            => new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
}
