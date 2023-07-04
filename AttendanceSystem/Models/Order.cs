using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models
{
    public class Order
    {
        [BsonElement("outlet_visit_id")]
        public string? OutletVisitId { get; set; }

        [BsonElement("user_id")]
        public string? UserId { get; set; }

        [BsonElement("username")]
        public string? Username { get; set; }

        [BsonElement("Outlet_name")]
        public string? OutletName { get; set; }

        [BsonElement("product_name")]
        public IEnumerable<Product> Products { get; set; } = new List<Product>();

        [BsonElement("product_count")]
        public string? ProductCount { get; set; }

        [BsonElement("total_amount")]
        public double TotalAmount { get; set; }

        [BsonElement("is_shipped")]
        public bool IsShipped { get; set; }
    }
}
