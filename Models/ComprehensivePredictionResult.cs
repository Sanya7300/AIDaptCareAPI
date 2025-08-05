using System.Collections.Generic;
using AIDaptCareAPI.Models;

namespace AIDaptCareAPI.Models
{
    public class ComprehensivePredictionResult
    {
        public List<string> Condition { get; set; }
        public string PredictedCondition { get; set; }
        public List<string> Diagnosis { get; set; }
        public List<string> Treatment { get; set; }
        public List<string> Remedies { get; set; }
        public List<string> Medicines { get; set; }
        public List<string> RecommendedTests { get; set; }
        public List<string> ResearchLinks { get; set; }
        public List<SymptomRecord> History { get; set; }
    }
}