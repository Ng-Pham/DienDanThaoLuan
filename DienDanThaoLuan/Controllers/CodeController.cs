using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Web.Razor.Parser.SyntaxConstants;
using DienDanThaoLuan.Models;

namespace DienDanThaoLuan.Controllers
{
    public class CodeController : Controller
    {
        // GET: Code
        [HttpGet]
        public ActionResult ExecutionResult()
        {
            return View();
        }
        private static readonly Dictionary<string, string> LANGUAGE_VERSIONS = new Dictionary<string, string>
        {
            { "python", "3.10.0" },
            { "javascript", "18.15.0" },
            { "csharp", "6.12.0" },
            { "java", "15.0.2" },
            { "cpp", "10.2.0" },
            { "c",  "10.2.0" },
             { "php", "8.2.3" }
        };

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> ExecuteAndDisplayResult(string language, string sourceCode, string input)
        {
            var url = "https://emkc.org/api/v2/piston/execute";

            // Chuẩn bị payload JSON
            var payload = new
            {
                language = language,
                version = LANGUAGE_VERSIONS.ContainsKey(language) ? LANGUAGE_VERSIONS[language] : "",
                files = new[] { new { content = sourceCode } },
                stdin = input // Thêm input vào payload
            };

            // Serialize payload thành JSON string sử dụng Newtonsoft.Json
            var jsonPayload = JsonConvert.SerializeObject(payload);

            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                string responseString = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<CodeExecutionResult>(responseString);

                if (result == null || result.run == null)
                {
                    // Xử lý trường hợp không có kết quả
                    ViewBag.Stdout = "Lỗi: Phản hồi từ API không hợp lệ.";
                    ViewBag.Stderr = "Lỗi: Phản hồi từ API không hợp lệ.";
                    ViewBag.CodeExitStatus = -1;
                }
                else
                {
                    ViewBag.Stdout = result.run.stdout;
                    ViewBag.Stderr = result.run.stderr;
                    ViewBag.CodeExitStatus = result.run.code;
                }
                // Truyền mã và kết quả vào ViewBag để hiển thị trong View
                ViewBag.CodeContent = sourceCode;
                ViewBag.CodeInput = input; // Truyền lại input để hiển thị
                ViewBag.SelectedLanguage = language; // Lưu ngôn ngữ được chọn
            }

            return View("ExecutionResult"); // Điều hướng đến view kết quả
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ExecuteAndDisplayHtml(string sourceCode, string language)
        {
            ViewBag.SelectedLanguage = language; // Lưu ngôn ngữ được chọn
            // Lưu mã HTML vào ViewBag để truyền cho View
            ViewBag.HtmlContent = sourceCode;

            // Chuyển hướng đến trang DisplayHtml.cshtml
            return View("DisplayHtml");
        }
    }
}