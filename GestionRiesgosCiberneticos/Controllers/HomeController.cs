using System.Diagnostics;
using GestionRiesgosCiberneticos.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GestionRiesgosCiberneticos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var activos = new List<CyberRiskManager.Models.Activo>(); // Fetch or initialize the list
            return View(activos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Details()
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
