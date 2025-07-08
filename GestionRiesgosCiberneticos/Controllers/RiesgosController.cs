using CyberRiskManager.Data;
using CyberRiskManager.Models;
using CyberRiskManager.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

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

            int[,] heatmap = new int[4, 4];
            foreach (var r in riesgos)
            {
                if (r.Probabilidad >= 1 && r.Probabilidad <= 3 && r.Impacto >= 1 && r.Impacto <= 3)
                {
                    heatmap[r.Probabilidad, r.Impacto]++;
                }
            }
            ViewBag.Heatmap = heatmap;

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
            
            if (!ObjectId.TryParse(riesgo.ActivoId, out _))
            {
                ModelState.AddModelError("ActivoId", "❌ Debes seleccionar un activo válido.");
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

        public IActionResult Edit(string id)
        {
            var riesgo = _mongo.GetRiesgos().FirstOrDefault(r => r.Id == id);
            if (riesgo == null)
                return NotFound();

            ViewBag.Activos = _mongo.GetAll();
            return View(riesgo);
        }

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

        public IActionResult Delete(string id)
        {
            var riesgo = _mongo.GetRiesgos().FirstOrDefault(r => r.Id == id);
            if (riesgo == null)
                return NotFound();

            return View(riesgo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            _mongo.DeleteRiesgo(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult SugerirControles(string term)
        {
            var sugerencias = _mongo.ObtenerControlesExistentesSugeridos(term);
            return Json(sugerencias);
        }

        [HttpGet]
        public async Task<IActionResult> Tratamiento(string id)
        {
            var riesgo = _mongo.GetRiesgos().FirstOrDefault(r => r.Id == id);
            if (riesgo == null) return NotFound();

            var activo = _mongo.GetById(riesgo.ActivoId);
            if (activo == null) return NotFound();

            var ia = HttpContext.RequestServices.GetRequiredService<IAService>();

            // 👇 Aquí corregimos: desestructuramos la tupla correctamente
            var (estrategia, justificacion, controles) = await ia.GenerarTratamientoIAAsync(riesgo, activo);

            riesgo.Estrategia = estrategia;
            riesgo.JustificacionTratamiento = justificacion;
            riesgo.ControlesPropuestos = controles;

            return View(riesgo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarTratamiento(string id, Riesgo riesgo, List<string> ControlesPropuestos)
        {
            var original = _mongo.GetRiesgos().FirstOrDefault(r => r.Id == id);
            if (original == null)
                return NotFound();

            original.Estrategia = riesgo.Estrategia;
            original.ControlesPropuestos = ControlesPropuestos ?? new();
            original.Responsable = riesgo.Responsable;
            original.FechaObjetivo = riesgo.FechaObjetivo;
            original.JustificacionTratamiento = riesgo.JustificacionTratamiento;

            // ✅ Cálculo automático de riesgo residual
            int riesgoInicial = original.Probabilidad * original.Impacto;
            int cantidadControles = original.ControlesPropuestos.Count;
            int reduccion = Math.Min(cantidadControles, 3); // máximo 3 de reducción
            original.RiesgoResidual = Math.Max(riesgoInicial - reduccion, 1); // mínimo 1

            _mongo.UpdateRiesgo(original);

            return RedirectToAction(nameof(Details), new { id = original.Id });
        }

        public IActionResult Details(string id)
        {
            var riesgo = _mongo.GetRiesgos().FirstOrDefault(r => r.Id == id);
            if (riesgo == null) return NotFound();

            var activo = _mongo.GetById(riesgo.ActivoId);
            ViewBag.NombreActivo = activo?.Nombre ?? "(desconocido)";
            ViewBag.TipoActivo = activo?.Tipo.ToString() ?? "";
            ViewBag.Observaciones = _mongo.GetObservacionesPorRiesgo(id);

            return View(riesgo);
        }


        public IActionResult TestInsert()
        {
            var riesgo = new Riesgo
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ActivoId = "66ae4565b486f79d14a12345", // ← USA un ObjectId válido que ya exista en tu colección Activos
                Amenaza = "Prueba directa",
                Vulnerabilidad = "Falta de prueba",
                ControlesExistentes = "Control X",
                Probabilidad = 2,
                Impacto = 2,
                Estrategia = "Mitigar",
                Responsable = "Prueba",
                FechaObjetivo = DateTime.UtcNow.AddDays(30),
                JustificacionTratamiento = "Solo prueba"
            };

            try
            {
                _mongo.AddRiesgo(riesgo);
                return Content("✅ Riesgo de prueba insertado.");
            }
            catch (Exception ex)
            {
                return Content($"❌ ERROR: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult AgregarObservacion(string RiesgoId, string Texto)
        {
            if (!string.IsNullOrWhiteSpace(Texto))
            {
                var obs = new Observacion
                {
                    RiesgoId = RiesgoId,
                    Texto = Texto,
                    Autor = User.Identity?.Name ?? "Anonimo"
                };
                _mongo.AgregarObservacion(obs);
            }
            return RedirectToAction("Details", new { id = RiesgoId });
        }

        public IActionResult Monitoreo()
        {
            var riesgos = _mongo.GetRiesgos();
            var activos = _mongo.GetAll().ToDictionary(a => a.Id, a => a.Nombre);
            ViewBag.Activos = activos;
            return View(riesgos);
        }

    }
}
