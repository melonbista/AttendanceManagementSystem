using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models
{
    public class User: BaseModel
    {

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("token")]
        public string Token { get; set; }

        [BsonElement("is_active")]
        public bool IsActive { get; set; }

        [BsonElement("token_issued_at")]
        public DateTime? TokenIssuedAt { get; set; }   
    }
}
