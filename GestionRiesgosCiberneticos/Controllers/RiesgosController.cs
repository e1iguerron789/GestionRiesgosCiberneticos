using Microsoft.AspNetCore.Mvc;
using CyberRiskManager.Data;
using CyberRiskManager.Models;

namespace CyberRiskManager.Controllers
{
    public class RiesgosController : Controller
    {
        private readonly MongoService _mongo;

        public RiesgosController(MongoService mongo)
        {
            _mongo = mongo;
        }

        public IActionResult Index()
        {
            var riesgos = _mongo.GetRiesgos();
            var activos = _mongo.GetAll();

            ViewBag.Activos = activos.ToDictionary(a => a.Id, a => a.Nombre);
            return View(riesgos);
        }

        public IActionResult Create()
        {
            ViewBag.Activos = _mongo.GetAll();
            return View(new Riesgo());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Riesgo riesgo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Activos = _mongo.GetAll();
                return View(riesgo);
            }

            _mongo.AddRiesgo(riesgo);
            return RedirectToAction(nameof(Index));
        }
    }
}
