using Microsoft.AspNetCore.Mvc;
using CyberRiskManager.Data;
using CyberRiskManager.Models;
using CyberRiskManager.Services;

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


        [HttpGet]
        public async Task<IActionResult> SugerenciasIA(string activoId, string activoNombre)
        {
            try
            {
                var activo = _mongo.GetById(activoId);
                if (activo == null)
                    return BadRequest(new { error = "Activo no encontrado" });

                var ia = HttpContext.RequestServices.GetRequiredService<IAService>();

                var (amenazas, vulnerabilidades) = await ia.SugerirAmenazasYVulnerabilidadesAsync(
                    activo.Tipo.ToString(),
                    activo.Confidencialidad,
                    activo.Integridad,
                    activo.Disponibilidad
                );

                return Json(new { amenazas, vulnerabilidades });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Fallo interno", detalle = ex.Message });
            }
        }
        // GET: Riesgos/Edit/5
        public IActionResult Edit(string id)
        {
            var riesgo = _mongo.GetRiesgos().FirstOrDefault(r => r.Id == id);
            if (riesgo == null)
                return NotFound();

            ViewBag.Activos = _mongo.GetAll();
            return View(riesgo);
        }

        // POST: Riesgos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Riesgo riesgo)
        {
            if (id != riesgo.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Activos = _mongo.GetAll();
                return View(riesgo);
            }

            _mongo.UpdateRiesgo(riesgo);
            return RedirectToAction(nameof(Index));
        }

        // GET: Riesgos/Delete/5
        public IActionResult Delete(string id)
        {
            var riesgo = _mongo.GetRiesgos().FirstOrDefault(r => r.Id == id);
            if (riesgo == null)
                return NotFound();

            return View(riesgo);
        }

        // POST: Riesgos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            _mongo.DeleteRiesgo(id);
            return RedirectToAction(nameof(Index));
        }


    }
}
