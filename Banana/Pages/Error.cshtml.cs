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
        public string Title { get; set; } = "����";
        public string ErrorMessage { get; set; } = "�����ˡ�";

        private void InitializeMessage(int c)
        {
            switch (c)
            {
                case 400:
                    Title = "400 - �������";
                    ErrorMessage = "�㳢������ֵĲ�����";
                    break;
                case 401:
                    Title = "401 - ��Ҫ��¼";
                    ErrorMessage = "��Ϊʲô�����";
                    break;
                case 403:
                    Title = "403 - ��ֹ����";
                    ErrorMessage = "�㿴�����ҡ�";
                    break;
                case 404:
                    Title = "404 - δ�ҵ�";
                    ErrorMessage = "�Ҳ����ڡ�";
                    break;
                case 405:
                    Title = "405 - ������ķ���";
                    ErrorMessage = "��Ҫ������ʲô��";
                    break;
                case 418:
                    Title = "418 - ���ǲ��";
                    ErrorMessage = "�Ұ������֡�";
                    break;
                case 500:
                    Title = "500 - �ڲ�����";
                    ErrorMessage = "�㷢�����ҵ� bug��";
                    break;
                case 501:
                    Title = "501 - ��ûд";
                    ErrorMessage = "������ܻ�ûд������";
                    break;
                case 503:
                    Title = "503 - ����ά��";
                    ErrorMessage = "���Ժ�������";
                    break;
                case 520:
                    Title = "520 - δ֪����";
                    ErrorMessage = "�Ұ��㡣";
                    break;
                default:
                    Title = c == -1 ? "����" : $"{c} - ����";
                    ErrorMessage = "�����ˡ�";
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
