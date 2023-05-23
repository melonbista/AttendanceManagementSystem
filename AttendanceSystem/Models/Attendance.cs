using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace AttendanceSystem.Model
{
    public class Attendance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }

        [BsonRequired]
        public DateTime? PunchInTime { get; set; }

        [BsonIgnoreIfNull]
        public double? PunchInLatitude { get; set; }

        [BsonIgnoreIfNull]
        public double? PunchInLongitude { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? PunchOutTime { get; set; }

        [BsonIgnoreIfNull]
        public double? PunchOutLatitude { get; set; }

        [BsonIgnoreIfNull]
        public double? PunchOutLongitude { get; set; }

        
    }
}