using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceSystem.Model
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRequired]
        public string? Name { get; set; }

        [BsonRequired]
        public string? Address { get; set; }

        [BsonRequired]
        public string? Email { get; set; }

        [BsonRequired]
        public string? Phone { get; set; }
    }
}
