using System.Diagnostics;
using ASC.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;
using ASC.Utilities;

namespace ASC.Web.Controllers
{
    public class HomeController : AnonymousController
    {
        private readonly ILogger<HomeController> _logger;
        private IOptions<ApplicationSettings> _settings;

        public HomeController(ILogger<HomeController> logger, IOptions<ApplicationSettings> settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public IActionResult Index()
        {
            // Lưu cấu hình _settings.Value vào Session với key là "Test"
            HttpContext.Session.SetSession("Test", _settings.Value);

            // Lấy ra lại từ Session để kiểm tra (chỉ là code test theo bài Lab)
            var settings = HttpContext.Session.GetSession<ApplicationSettings>("Test");

            // Truyền ApplicationTitle sang View
            ViewBag.Title = _settings.Value.ApplicationTitle;

            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}