using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceSystem.Models
{
    public class OutletVisit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? OutletId { get; set; }
        public string? UserId { get; set; }
        public DateTime? CheckInTime { get; set; }

        public double? CheckInLatitude { get; set; }

        public double? CheckInLongitude { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public double? CheckOutLatitude { get; set; }

        public double? CheckOutLongitude { get; set; }
    }
}
