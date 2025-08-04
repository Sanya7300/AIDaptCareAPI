using AIDaptCareAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AIDaptCareAPI.Services
{
    public class MedicalReportService
    {
        private readonly IMongoCollection<MedicalReport> _reports;
        public MedicalReportService(IConfiguration config, IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var db = client.GetDatabase(dbSettings.Value.DatabaseName);
            _reports = db.GetCollection<MedicalReport>("MedicalReports");
        }
        public async Task CreateAsync(MedicalReport report)
        {
            await _reports.InsertOneAsync(report);
        }
        public async Task<List<MedicalReport>> GetUserReportsAsync(string username)
        {
            return await _reports.Find(r => r.UserId == username).ToListAsync();
        }
    }
}