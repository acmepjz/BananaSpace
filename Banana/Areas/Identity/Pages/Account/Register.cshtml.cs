using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Banana.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Banana.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<BananaUser> _signInManager;
        private readonly UserManager<BananaUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<BananaUser> userManager,
            SignInManager<BananaUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "{0}不能为空。")]
            [StringLength(16, ErrorMessage = "{0}长度必须介于 {2}–{1} 字符之间。", MinimumLength = 4)]
            [RegularExpression("^[^<>&%/\\\\'\"^$\\(\\)\\[\\]\\{\\}\\s]+$", ErrorMessage = "用户名不能包含特殊字符或空格。")]
            [Display(Name = "用户名")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "{0}不能为空。")]
            [EmailAddress(ErrorMessage = "不是有效的电子邮箱地址。")]
            [Display(Name = "电子邮箱")]
            public string Email { get; set; }

            [Required(ErrorMessage = "{0}不能为空。")]
            [StringLength(64, ErrorMessage = "{0}长度必须介于 {2}–{1} 字符之间。", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "密码")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "确认密码")]
            [Compare("Password", ErrorMessage = "两次输入的密码不一致。")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new BananaUser { UserName = Input.UserName, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.Page(
                    //    "/Account/ConfirmEmail",
                    //    pageHandler: null,
                    //    values: new { userId = user.Id, code = code },
                    //    protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
