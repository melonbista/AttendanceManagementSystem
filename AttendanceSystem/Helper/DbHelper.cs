using AttendanceManagementSystem.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace AttendanceManagementSystem.Helper
{
    public class DbHelper
    {
        public enum Direction
        {
            Ascending,
            Descending
        };

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
                {typeof(Product),"product" },
                {typeof(Brand),"brand" },
                {typeof(Vertical),"vertical" },
                {typeof(Division),"division" },
                {typeof(Unit),"unit" },
            };
        }

        public IMongoCollection<TDocument> GetCollection<TDocument>()
            where TDocument : BaseModel
        {
            string collectionName = _collectionName[typeof(TDocument)];
            return _database.GetCollection<TDocument>(collectionName);
        }

        public FilterDefinition<TDocument> LikeFilter<TDocument>(Expression<Func<TDocument, object>> field, string value)
        {
            return Builders<TDocument>.Filter.Regex(field, new BsonRegularExpression(value, "i"));
        }

        public SortDefinition<TDocument> GetSortDefinition<TDocument>(Expression<Func<TDocument,object>> field,Direction direction)
        {
            if(direction == Direction.Ascending)
            {
                return Builders<TDocument>.Sort.Ascending(field);
            }
            else
            {
                return Builders<TDocument>.Sort.Descending(field);
            }
        }

        public bool IdExists<TDocument>(string id, FilterDefinition<TDocument> filter)
            where TDocument : BaseModel
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return false;
            }

            return RecordExists(Builders<TDocument>.Filter.Eq(x => x.Id, id) & filter);
        }

        public async Task<bool> IdExistsAsync<TDocument>(string id,FilterDefinition<TDocument> filter,CancellationToken cancellationToken=default)
            where TDocument : BaseModel
        {
            if(!ObjectId.TryParse(id, out _))
            {
                return false;
            }
            return await RecordExistsAsync(Builders<TDocument>.Filter.Eq(x=>x.Id, id) & filter, cancellationToken);
        }

        public bool RecordExists<TDocument>(FilterDefinition<TDocument> filter)
            where TDocument : BaseModel
        {
            var collection = GetCollection<TDocument>();

            return collection.Find(filter).Project(x => x.Id).FirstOrDefault() is not null;
        }

        public async Task<bool> RecordExistsAsync<TDocument>(FilterDefinition<TDocument> filter,CancellationToken cancellationToken=default)
            where TDocument : BaseModel
        {
            var collection = GetCollection<TDocument>();
            return await collection.Find(filter).Project(x => x.Id).FirstOrDefaultAsync(cancellationToken) is not null;
        }

        public async Task<TNewProjection?> FindByIdAsync<TDocument, TNewProjection>(
            string id,
            Expression<Func<TDocument, TNewProjection>> projection,
            FilterDefinition<TDocument> filter,
            CancellationToken cancellationToken = default)
            where TDocument : BaseModel
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return default;
            }

            var collection = GetCollection<TDocument>();

            return await collection
                .Find(Builders<TDocument>.Filter.Eq(x => x.Id, id) & filter)
                .Project(projection)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Dictionary<object, TNewProjection>> GetParentModels<TModel, TDocument, TNewProjection>(
            IEnumerable<TModel> models,
            Func<TModel, object> localField,
            Expression<Func<TDocument, object>> foreignField,
            Expression<Func<TDocument, TNewProjection>> projection,
            Func<TNewProjection, object> keySelector,
            FilterDefinition<TDocument> filter,
            CancellationToken cancellationToken = default)
            where TDocument : BaseModel
        {
            var ids = models.Select(localField).Where(x => x is not null).Distinct();

            var collection = GetCollection<TDocument>();

            var list = await collection
                .Find(Builders<TDocument>.Filter.In(foreignField, ids) & filter)
                .Project(projection)
                .ToListAsync(cancellationToken);

            return list.ToDictionary(keySelector, x => x);
        }

        public async Task<Dictionary<object, TNewProjection>> GetParentModels<TModel, TDocument, TNewProjection>(
            IEnumerable<TModel> models,
            Func<TModel, IEnumerable<object>> localField,
            Expression<Func<TDocument, object>> foreignField,
            Expression<Func<TDocument, TNewProjection>> projection,
            Func<TNewProjection, object> keySelector,
            FilterDefinition<TDocument> filter,
            CancellationToken cancellationToken = default)
            where TDocument : BaseModel
        {
            var ids = models.SelectMany(localField).Where(x => x is not null).Distinct();

            var collection = GetCollection<TDocument>();

            var list = await collection
                .Find(Builders<TDocument>.Filter.In(foreignField, ids) & filter)
                .Project(projection)
                .ToListAsync(cancellationToken);

            return list.ToDictionary(keySelector, x => x);
        }
    }
}


