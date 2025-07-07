using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CyberRiskManager.Models
{
    public class Riesgo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        [BsonRepresentation(BsonType.ObjectId)]
        public string ActivoId { get; set; } = "";

        public string Amenaza { get; set; } = "";
        public string Vulnerabilidad { get; set; } = "";
        public string ControlesExistentes { get; set; } = "";

        public int Probabilidad { get; set; } = 2;  // 1 a 3
        public int Impacto { get; set; } = 2;       // 1 a 3

        [BsonIgnore]
        public int NivelRiesgo => Probabilidad * Impacto;

        [BsonIgnore]
        public string Prioridad
        {
            get
            {
                // Sugerencia básica de prioridad
                if (NivelRiesgo >= 7)
                    return "Alta prioridad 🔥";
                else if (NivelRiesgo >= 4)
                    return "Media prioridad ⚠️";
                else
                    return "Baja prioridad ✅";
            }
        }
    }
}
