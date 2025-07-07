using CyberRiskManager.Data;
using CyberRiskManager.Models;
using GestionRiesgosCiberneticos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CyberRiskManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MongoService _mongo;

        public HomeController(ILogger<HomeController> logger, MongoService mongo)
        {
            _logger = logger;
            _mongo = mongo;
        }

        // GET: / or /Home/Index
        public IActionResult Index()
        {
            // Trae todos los activos desde MongoDB
            var activos = _mongo.GetAll();
            return View(activos);
        }

        // GET: /Home/Details/{id}
        public IActionResult Details(string id)
        {
            var activo = _mongo.GetById(id);
            if (activo == null) return NotFound();
            return View(activo);
        }

        // GET: /Home/Create
        public IActionResult Create()
        {
            ViewBag.Sugerencias = new List<string>
            {
                "Servidor", "Base de Datos", "Aplicación Web", "Correo Institucional"
            };
            return View(new Activo());
        }

        // POST: /Home/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Activo activo)
        {
            if (!ModelState.IsValid) return View(activo);

            _mongo.Add(activo);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Home/Edit/{id}
        public IActionResult Edit(string id)
        {
            var activo = _mongo.GetById(id);
            if (activo == null) return NotFound();
            return View(activo);
        }

        // POST: /Home/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Activo activo)
        {
            if (id != activo.Id) return BadRequest();
            if (!ModelState.IsValid) return View(activo);

            _mongo.Update(activo);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Home/Delete/{id}
        public IActionResult Delete(string id)
        {
            var activo = _mongo.GetById(id);
            if (activo == null) return NotFound();
            return View(activo);
        }

        // POST: /Home/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            _mongo.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
