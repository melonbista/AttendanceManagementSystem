using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models
{
    public class Vertical : BaseModel
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("division_id")]
        public string DivisionId { get; set; }
    }
}
