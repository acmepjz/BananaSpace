using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Banana.Pages
{
    public static class Actions
    {
        public static IActionResult RedirectToLoginPage() 
            => new LocalRedirectResult("~/Identity/Account/Login");

        public static IActionResult RedirectToUserPage()
            => new LocalRedirectResult("~/user");

        public static IActionResult RedirectToErrorPage()
            => new LocalRedirectResult("~/Error");

        public static IActionResult Status200()
            => new OkResult();

        public static IActionResult Status418()
            => new StatusCodeResult(StatusCodes.Status418ImATeapot); // 418 is for error msgs not yet implemented.

        public static IActionResult Status500()
            => new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
}
