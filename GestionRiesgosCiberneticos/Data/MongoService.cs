using CyberRiskManager.Models;
using CyberRiskManager.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;


namespace CyberRiskManager.Data
{
    public class MongoService
    {
        private readonly ILogger<MongoService> _logger;
        private readonly IMongoCollection<Activo> _activos;
        private readonly IMongoCollection<Riesgo> _riesgos;
        private readonly IMongoCollection<Observacion> _observaciones;


        public MongoService(IConfiguration config, ILogger<MongoService> logger)
        {
            _logger = logger;
            try
            {
                var conn = config.GetConnectionString("MongoDB");
                _logger.LogInformation("Conectando a MongoDB con: {0}", conn);
                var client = new MongoClient(conn);
                var db = client.GetDatabase("GestionSeguridad");
                _activos = db.GetCollection<Activo>("Activos");
                _riesgos = db.GetCollection<Riesgo>("Riesgos");
                _logger.LogInformation("Conexión a MongoDB establecida.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error conectando a MongoDB");
                throw;
            }
            
        }
        

        public List<Activo> GetAll() => _activos.Find(a => true).ToList();
        public Activo? GetById(string id) => _activos.Find(a => a.Id == id).FirstOrDefault();
        public void Add(Activo activo)
        {
            _activos.InsertOne(activo);
            _logger.LogInformation("Activo insertado Id={Id}", activo.Id);
        }
        public void Update(Activo activo) => _activos.ReplaceOne(a => a.Id == activo.Id, activo);
        public void Delete(string id) => _activos.DeleteOne(a => a.Id == id);

        public List<Riesgo> GetRiesgos() => _riesgos.Find(r => true).ToList();
        public List<Riesgo> GetRiesgosPorActivo(string activoId) =>
            _riesgos.Find(r => r.ActivoId == activoId).ToList();

        public void AddRiesgo(Riesgo riesgo)
        {
            _logger.LogInformation("Insertando riesgo: {@riesgo}", riesgo);
            _riesgos.InsertOne(riesgo);
        }

        public void UpdateRiesgo(Riesgo riesgo)
        {
            _riesgos.ReplaceOne(r => r.Id == riesgo.Id, riesgo);
        }

        public void DeleteRiesgo(string id)
        {
            _riesgos.DeleteOne(r => r.Id == id);
        }

        public List<string> ObtenerControlesExistentesSugeridos(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                filtro = "";

            var riesgos = _riesgos.Find(_ => true).ToList();
            return riesgos
                .Select(r => r.ControlesExistentes)
                .Where(c => !string.IsNullOrWhiteSpace(c) && c.ToLower().Contains(filtro.ToLower()))
                .Distinct()
                .Take(10)
                .ToList();
        }

        public void AgregarObservacion(Observacion o) => _observaciones.InsertOne(o);

        public List<Observacion> GetObservacionesPorRiesgo(string riesgoId) =>
            _observaciones.Find(o => o.RiesgoId == riesgoId).ToList();




    }
}
