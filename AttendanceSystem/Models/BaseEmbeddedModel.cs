using MongoDB.Bson;

namespace AttendanceManagementSystem.Models
{
    public abstract class BaseEmbeddedModel:BaseModel
    {
        public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    }
}
