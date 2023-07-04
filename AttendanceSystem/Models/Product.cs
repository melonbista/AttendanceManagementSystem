using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProductId { get; set; }

        [BsonElement("product_name")]
        public string? ProductName { get; set; }

        [BsonElement("brand_name")]
        public string? BrandName { get; set; }

        [BsonElement("price")]
        public string? Price { get; set; }
    }
}
