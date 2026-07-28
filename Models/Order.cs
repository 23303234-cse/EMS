using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EMS.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        public List<OrderItem> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }

        // Order Status
        public string Status { get; set; } = "Pending";

        // Payment Information
        public string PaymentMethod { get; set; } = "Cash on Delivery";

        public string PaymentStatus { get; set; } = "Pending";

        // Order Date
        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
}