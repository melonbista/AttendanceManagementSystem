using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Model
{
    public class Outlet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? OutletId { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Address { get; set; }
        [EmailAddress]
        public string? OwnerEmail { get; set; }
        [EmailAddress]
        public string? OwnerPhone { get; set; }
        [Phone]
        public string? OutletPhone { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [Required]
        public string? UserId { get; set; }
    }
}