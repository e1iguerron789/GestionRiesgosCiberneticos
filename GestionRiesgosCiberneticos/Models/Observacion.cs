using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CyberRiskManager.Models
{
    public class Observacion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string RiesgoId { get; set; }

        public string Autor { get; set; } = "Analista";
        public string Texto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
