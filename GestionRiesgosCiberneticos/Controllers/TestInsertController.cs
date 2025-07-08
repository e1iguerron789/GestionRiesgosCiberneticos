
using CyberRiskManager.Data;
using CyberRiskManager.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace CyberRiskManager.Controllers
{
    public class TestInsertController : Controller
    {
        private readonly MongoService _mongo;

        public TestInsertController(MongoService mongo)
        {
            _mongo = mongo;
        }

        public IActionResult Index()
        {
            // Limpia datos anteriores (opcional)
            var existentes = _mongo.GetAll();
            foreach (var a in existentes)
                _mongo.Delete(a.Id);

            var riesgosAntiguos = _mongo.GetRiesgos();
            foreach (var r in riesgosAntiguos)
                _mongo.DeleteRiesgo(r.Id);

            // Insertar 10 activos
            var activos = new List<Activo>();
            for (int i = 1; i <= 10; i++)
            {
                var activo = new Activo
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Nombre = $"Activo #{i}",
                    Tipo = (TipoActivo)(i % 5),
                    Descripcion = $"Descripción del activo número {i}",
                    Confidencialidad = i % 3 + 1,
                    Integridad = (i + 1) % 3 + 1,
                    Disponibilidad = (i + 2) % 3 + 1
                };
                _mongo.Add(activo);
                activos.Add(activo);
            }

            // Insertar 10 riesgos
            var amenazas = new[] { "Phishing", "Ransomware", "DDoS", "Acceso no autorizado", "Error humano" };
            var vulnerabilidades = new[] { "Falta de capacitación", "Puertos abiertos", "Sin autenticación 2FA", "Contraseñas débiles", "Backups ausentes" };

            var rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                var riesgo = new Riesgo
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    ActivoId = activos[rand.Next(activos.Count)].Id,
                    Amenaza = amenazas[rand.Next(amenazas.Length)],
                    Vulnerabilidad = vulnerabilidades[rand.Next(vulnerabilidades.Length)],
                    ControlesExistentes = "Antivirus, Firewall",
                    Probabilidad = rand.Next(1, 4),
                    Impacto = rand.Next(1, 4),
                    Estrategia = "",
                    Responsable = "",
                    JustificacionTratamiento = ""
                };
                _mongo.AddRiesgo(riesgo);
            }

            return Content("✅ Insertados 10 activos y 10 riesgos con éxito.");
        }
    }
}
