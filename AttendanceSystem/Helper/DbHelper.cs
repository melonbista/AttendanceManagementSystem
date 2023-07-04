using AttendanceManagementSystem.Models;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Helper
{

    public class DbHelper
    {
        private readonly IMongoDatabase _database;
        private readonly Dictionary<Type, string> _collectionName;

        public DbHelper(IMongoDatabase database)
        {
            _database = database;
            _collectionName = new Dictionary<Type, string>()
            {
                {typeof(Attendance),"attendance" },
                {typeof(Outlet),"outlet" },
                {typeof(User),"user" },
                {typeof(OutletVisit),"outletVisit" },
                {typeof(Order),"order"},
                {typeof(Product),"product" }
            };
        }

        public IMongoCollection<TDocument> GetCollection<TDocument>()
        {
            string collectionName = _collectionName[typeof(TDocument)];
            return _database.GetCollection<TDocument>(collectionName);
        }

        //public MongoDbContext(IOptions<DatabaseSettings> dbSetting)
        //{
        //    var mongoClient = new MongoClient(dbSetting.Value.ConnectionString);
        //    _database = mongoClient.GetDatabase(dbSetting.Value.DatabaseName);

        //}
        //public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        //public IMongoCollection<Outlet> Outlets => _database.GetCollection<Outlet>("outlets");
        //public IMongoCollection<Attendance> Attendances => _database.GetCollection<Attendance>("attendances");
        //public IMongoCollection<OutletVisit> OutletVisits => _database.GetCollection<OutletVisit>("outletVisits");       
        //public IMongoCollection<Order>  => _database.GetCollection<OutletVisit>("outletVisits");       
    }
}

