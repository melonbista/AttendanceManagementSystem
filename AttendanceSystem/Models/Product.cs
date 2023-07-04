using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models
{
    public class Product : BaseEmbeddedModel
    {
        [BsonElement("division_id")]
        public string DivisionId { get; set; }

        [BsonElement("vertical_id")]
        public string VerticalId { get; set; }

        [BsonElement("unit_id")]
        public string UnitId { get; set; }

        [BsonElement("product_name")]
        public string Name { get; set; }

        [BsonElement("brand_id")]
        public string BrandId { get; set; }

        [BsonElement("brand_name")]
        public string BrandName { get; set; }

        [BsonElement("distributed_selling_price")]
        public float DistibutedSellingPrice { get; set; }

        [BsonElement("retailer_selling_price")]
        public float RetailerSellingPrice { get; set; }

        [BsonElement("super_distributor_selling_price")]
        public float SuperDistributorSellingPrice { get; set; }

        [BsonElement("super_distributor_landing_price")]
        public float SuperDistributorLandingPrice { get; set; }

        [BsonElement("is_featured")]
        public bool IsFeatured { get; set; }

        [BsonElement("is_active")]
        public bool IsActive { get; set; }
    }
}
