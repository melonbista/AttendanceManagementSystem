using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models
{
    public class Outlet : BaseModel
    {
        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("owner_email")]
        public string? OwnerEmail { get; set; }

        [BsonElement("owner_phone")]
        public string? OwnerPhone { get; set; }

        [BsonElement("outlet_phone")]
        public string? OutletPhone { get; set; }

        [BsonElement("latitude")]
        public double? Latitude { get; set; }

        [BsonElement("longitude")]
        public double? Longitude { get; set; }
    }
}