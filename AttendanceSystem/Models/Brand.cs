using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models
{
    public class Brand : BaseModel
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("division_id")]
        public string DivisionId { get; set; }

        [BsonElement("vertical_id")]
        public string VerticalId { get; set; }

    }
}
