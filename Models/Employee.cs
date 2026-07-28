using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EMS.Models
{
    public class Employee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Department { get; set; } = null!;

        public decimal Salary { get; set; }
    }
}