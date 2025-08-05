using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
namespace AIDaptCareAPI.Models
{
    public class SymptomRecord
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("username")]
        public string Username { get; set; }
        [BsonElement("symptoms")]
        public List<string> Symptoms { get; set; }
        [BsonElement("reportText")]
        public string ReportText { get; set; }
        [BsonElement("condition")]
        public List<string> Condition { get; set; }
        [BsonElement("predictedCondition")]
        public string PredictedCondition { get; set; }
        [BsonElement("diagnosis")]
        public List<string> Diagnosis { get; set; }
        [BsonElement("treatment")]
        public List<string> Treatment { get; set; }
        [BsonElement("medicines")]
        public List<string> Medicines { get; set; }
        [BsonElement("remedies")]
        public List<string> Remedies { get; set; }
        [BsonElement("embedding")]
        public List<float> Embedding { get; set; }
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}