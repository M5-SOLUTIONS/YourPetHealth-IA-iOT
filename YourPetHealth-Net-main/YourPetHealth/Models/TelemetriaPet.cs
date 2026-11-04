using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YourPetHealth.Models
{
    public class TelemetriaPet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        [JsonIgnore]
        public string Id { get; set; } = string.Empty;

        public int PetId { get; set; }

        public double BatimentosCardiacos { get; set; }

        public double Temperatura { get; set; }

        public string StatusGeral { get; set; } = string.Empty;

        public DateTime DataLeitura { get; set; } = DateTime.UtcNow;
    }
}