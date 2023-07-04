using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models
{
    public class Order : BaseModel
    {
        [BsonElement("Outlet_visit_id")]
        public string OutletVisitId { get; set; }

        [BsonElement("outlet_id")]
        public string OutletId { get; set; }

        [BsonElement("outlet_name")]
        public string OutletName { get; set; }
        
        [BsonElement("user_id")]
        public string UserId { get; set; }
    }
}
