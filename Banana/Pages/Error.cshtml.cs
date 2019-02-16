using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Banana.Pages
{
    public class ErrorModel : PageModel
    {
        public string Title { get; set; } = "错误";
        public string ErrorMessage { get; set; } = "出错了。";

        private void InitializeMessage(int c)
        {
            switch (c)
            {
                case 400:
                    Title = "400 - 请求错误";
                    ErrorMessage = "你尝试了奇怪的操作。";
                    break;
                case 401:
                    Title = "401 - 需要登录";
                    ErrorMessage = "你为什么在这里？";
                    break;
                case 403:
                    Title = "403 - 禁止访问";
                    ErrorMessage = "你看不见我。";
                    break;
                case 404:
                    Title = "404 - 未找到";
                    ErrorMessage = "我不存在。";
                    break;
                case 405:
                    Title = "405 - 不允许的方法";
                    ErrorMessage = "你要对我做什么？";
                    break;
                case 418:
                    Title = "418 - 我是茶壶";
                    ErrorMessage = "我矮矮胖胖。";
                    break;
                case 500:
                    Title = "500 - 内部错误";
                    ErrorMessage = "你发现了我的 bug。";
                    break;
                case 501:
                    Title = "501 - 还没写";
                    ErrorMessage = "这个功能还没写出来。";
                    break;
                case 503:
                    Title = "503 - 正在维护";
                    ErrorMessage = "请稍后再来。";
                    break;
                case 520:
                    Title = "520 - 未知错误";
                    ErrorMessage = "我爱你。";
                    break;
                default:
                    Title = c == -1 ? "错误" : $"{c} - 错误";
                    ErrorMessage = "出错了。";
                    break;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult OnGet(int c = -1)
        {
            if (c != -1 && !(c >= 400 && c < 599))
                return NotFound();
            InitializeMessage(c);
            return Page();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult OnPost(int c = -1)
        {
            if (c != -1 && !(c >= 400 && c < 599))
                return NotFound();
            InitializeMessage(c);
            return Page();
        }
    }
}
