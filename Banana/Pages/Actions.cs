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
    }
}
