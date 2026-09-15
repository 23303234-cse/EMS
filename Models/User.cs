using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EMS.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        // Confirm Password is only for registration
        // It will NOT be saved in MongoDB
        [BsonIgnore]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public string? ProfileImage { get; set; }
    }
}