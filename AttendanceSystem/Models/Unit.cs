using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models
{
    public class Unit : BaseModel
    {
        [BsonElement("name")]
        public string Name { get; set; }
    }
}
