using CyberRiskManager.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CyberRiskManager.Data
{
    public class MongoService
    {
        private readonly IMongoCollection<Activo> _activos;
        private readonly IMongoCollection<Riesgo> _riesgos;
        private readonly IMongoCollection<Observacion> _observaciones;

        public MongoService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDB"));
            var database = client.GetDatabase("GestionSeguridad");

            _activos = database.GetCollection<Activo>("Activos");
            _riesgos = database.GetCollection<Riesgo>("Riesgos");

            _observaciones = database.GetCollection<Observacion>("Observaciones");
        }

        // ACTIVOS
        public List<Activo> GetAll() => _activos.Find(_ => true).ToList();
        public Activo GetById(string id) => _activos.Find(a => a.Id == id).FirstOrDefault();
        public void Add(Activo activo) => _activos.InsertOne(activo);
        public void Update(Activo activo) => _activos.ReplaceOne(a => a.Id == activo.Id, activo);
        public void Delete(string id) => _activos.DeleteOne(a => a.Id == id);

        // RIESGOS

        public List<Riesgo> GetRiesgos() => _riesgos.Find(_ => true).ToList();
        public void AddRiesgo(Riesgo riesgo) => _riesgos.InsertOne(riesgo);
        public void UpdateRiesgo(Riesgo riesgo) => _riesgos.ReplaceOne(r => r.Id == riesgo.Id, riesgo);
        public void DeleteRiesgo(string id) => _riesgos.DeleteOne(r => r.Id == id);

        // OBSERVACIONES
        public List<Observacion> GetObservacionesPorRiesgo(string riesgoId)
            => _observaciones.Find(o => o.RiesgoId == riesgoId).ToList();

        public void AgregarObservacion(Observacion o) => _observaciones.InsertOne(o);

        // AUTOCOMPLETE CONTROLES
        public List<string> ObtenerControlesExistentesSugeridos(string term)
        {
            return _riesgos
                .Find(r => r.ControlesExistentes.ToLower().Contains(term.ToLower()))
                .Project(r => r.ControlesExistentes)
                .ToList()
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .SelectMany(c => c.Split(','))
                .Select(c => c.Trim())
                .Where(c => c.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .Take(10)
                .ToList();
        }
    }
}
