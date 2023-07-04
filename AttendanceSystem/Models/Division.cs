using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models
{
    public class Division:BaseModel
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("abbreviation")]
        public string Abbreviation { get; set; }
    }
}
