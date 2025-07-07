using Microsoft.AspNetCore.Mvc;
using CyberRiskManager.Data;
using CyberRiskManager.Models;

using Microsoft.AspNetCore.Mvc;
using CyberRiskManager.Data;
using CyberRiskManager.Models;
using Microsoft.Extensions.Logging;

namespace CyberRiskManager.Controllers
{
    public class ActivosController : Controller
    {
        private readonly MongoService _mongo;
        private readonly ILogger<ActivosController> _logger;

        public ActivosController(MongoService mongo, ILogger<ActivosController> logger)
        {
            _mongo = mongo;
            _logger = logger;
        }

        // GET: /Activos
        public IActionResult Index()
        {
            var activos = _mongo.GetAll();
            return View(activos);
        }

        // GET: /Activos/Suggest?term=...
        [HttpGet]
        public IActionResult Suggest(string term)
        {
            var sugerencias = _mongo
                .GetAll()
                .Select(a => a.Nombre)
                .Where(n => n.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .Take(10)
                .ToList();
            return Json(sugerencias);
        }

        // GET: /Activos/Details/{id}
        public IActionResult Details(string id)
        {
            var activo = _mongo.GetById(id);
            if (activo == null) return NotFound();
            return View(activo);
        }

        // GET: /Activos/Create
        public IActionResult Create()
        {
            return View(new Activo());
        }

        // POST: /Activos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Activo activo)
        {
            if (!ModelState.IsValid) return View(activo);
            _mongo.Add(activo);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Activos/Edit/{id}
        public IActionResult Edit(string id)
        {
            var activo = _mongo.GetById(id);
            if (activo == null) return NotFound();
            return View(activo);
        }

        // POST: /Activos/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Activo activo)
        {
            if (id != activo.Id) return BadRequest();
            if (!ModelState.IsValid) return View(activo);
            _mongo.Update(activo);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Activos/Delete/{id}
        public IActionResult Delete(string id)
        {
            var activo = _mongo.GetById(id);
            if (activo == null) return NotFound();
            return View(activo);
        }

        // POST: /Activos/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            _mongo.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
