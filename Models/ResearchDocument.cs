using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace AIDaptCareAPI.Models
{
    public class ResearchDocument
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }
        [BsonElement("Title")]
        public string Title { get; set; }
        [BsonElement("Summary")]
        public string Summary { get; set; }
        [BsonElement("Content")]
        public string Content { get; set; }
        [BsonElement("Url")]
        public string Url { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("Embedding")]
        public List<float> Embedding { get; set; } = new List<float>();
        [BsonElement("UploadedAt")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}