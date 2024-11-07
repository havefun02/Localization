using Localization.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Localization.Controllers
{
    public class HomeController:Controller
    {
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IGreetingService _greetingService;

        public HomeController(IStringLocalizer<HomeController> localizer,IGreetingService greetingService)
        {
            _greetingService=greetingService;
            _localizer = localizer;
        }
        public IActionResult Index()
        {
            var route = HttpContext.Request.Path;
            string welcomeMessage = _localizer.GetString("greeting");
            string errorMessage = _localizer["greetingError"];
            string currentDate = DateTime.Now.ToString("D", CultureInfo.CurrentCulture); // "D" for long date format

            ViewData["Message"] = welcomeMessage;
            ViewData["Date"] = currentDate;
            ViewData["Error"] = errorMessage;
            return View();
        }
    }
}
