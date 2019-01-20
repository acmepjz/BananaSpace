using Microsoft.AspNetCore.Mvc;
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

        public static IActionResult RedirectTo404Page()
            => throw new NotImplementedException();
    }
}
