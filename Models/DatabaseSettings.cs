namespace AIDaptCareAPI.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UserCollectionName { get; set; }
        public string SymptomCollectionName { get; set; }
        public string ResearchCollectionName {  get; set; }
    }
}