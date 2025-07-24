using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace AIDaptCareAPI.Models
{
    public class SymptomRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("username")]
        public string? Username { get; set; }
        [BsonElement("symptoms")]
        public List<string> Symptoms { get; set; } = new();
        [BsonElement("predictedCondition")]
        public string? PredictedCondition { get; set; }
        [BsonElement("remedies")]
        public List<string>? Remedies { get; set; }
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}