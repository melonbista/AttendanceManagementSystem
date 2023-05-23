using AttendanceSystem.Model;
using AttendanceSystem.Models;
using AttendanceSystem.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AttendanceSystem.Settings
{

    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<DatabaseSettings> dbSetting)
        {
            var mongoClient = new MongoClient(dbSetting.Value.ConnectionString);
            _database = mongoClient.GetDatabase(dbSetting.Value.DatabaseName);

        }
        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<Outlet> Outlets => _database.GetCollection<Outlet>("outlets");
        public IMongoCollection<Attendance> Attendances => _database.GetCollection<Attendance>("attendances");
        public IMongoCollection<OutletVisit> OutletVisits => _database.GetCollection<OutletVisit>("outletVisits");
    }
}

