using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EMS.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string MessageText { get; set; } = string.Empty;

        public string Reply { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RepliedAt { get; set; }

        public string Status { get; set; } = "Pending";
    }
}