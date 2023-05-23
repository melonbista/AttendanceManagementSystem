using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Model
{
    public class Attendance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [Required]
        public string? UserId { get; set; }
        public DateTime? PunchInTime { get; set; }
        public bool Status { get; set; }
        public double? PunchInLatitude { get; set; }
        public double? PunchInLongitude { get; set; }
        public DateTime? PunchOutTime { get; set; }
        public double? PunchOutLatitude { get; set; }
        public double? PunchOutLongitude { get; set; }

        
    }
}