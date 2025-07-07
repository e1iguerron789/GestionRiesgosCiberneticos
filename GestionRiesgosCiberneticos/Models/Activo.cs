using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CyberRiskManager.Models
{
    public enum TipoActivo
    {
        Hardware,
        Software,
        Datos,
        Servicios,
        Personas
    }

    public class Activo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        public string Nombre { get; set; } = "";
        public TipoActivo Tipo { get; set; }
        public string? Descripcion { get; set; }

        public int Confidencialidad { get; set; } = 2;
        public int Integridad { get; set; } = 2;
        public int Disponibilidad { get; set; } = 2;

        public int Criticidad => (Confidencialidad + Integridad + Disponibilidad) / 3;
    }
}
