using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models
{
    public class OutletVisit : BaseModel
    {
        public string? OutletId { get; set; }
        public string? UserId { get; set; }
        public bool Status { get; set; }
        public DateTime? CheckInTime { get; set; }
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }
    }
}
