using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceSystem.Model
{
    public class Outlet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? OutletId { get; set; }

        [BsonRequired]
        public string? Name { get; set; }

        [BsonRequired]
        public string? Address { get; set; }

        [BsonIgnoreIfNull]
        public string? OwnerEmail { get; set; }

        [BsonRequired]
        public string? OwnerPhone { get; set; }

        [BsonIgnoreIfNull]
        public string? OutletPhone { get; set; }

        [BsonIgnoreIfNull]
        public double? Latitude { get; set; }

        [BsonIgnoreIfNull]
        public double? Longitude { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }
    }
}