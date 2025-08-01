using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
namespace AIDaptCareAPI.Models
{
    public class MedicalReport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }  // Optional, if users are tracked
        public string FileName { get; set; }
        public string ExtractedText { get; set; }
        public string Diagnosis { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}