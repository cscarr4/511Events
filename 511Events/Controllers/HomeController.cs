using _511Events.Logic;
using _511Events.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace _511Events.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EventGatherer gatherer;

        public HomeController(
            ILogger<HomeController> logger, 
            EventGatherer gatherer)
        {
            _logger = logger;
            this.gatherer = gatherer;
        }

        public IActionResult Index()
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

        [HttpPost]
        public async Task<IActionResult> GatherEvents(BeginProcessRequest request)
        {
            await gatherer.GatherEvents(request.AccountName, request.AccountKey);
            return new OkResult();
        }
    }
}