using Delivera.Models;

namespace Delivera.DTOs
{
    public class UpdateOrderStatusRequest
    {

        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}