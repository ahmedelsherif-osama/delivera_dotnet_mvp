using Delivera.Models;

namespace Delivera.DTOs
{
    public class OrderRequest
    {

        public Guid OrganizationId { get; set; }
        public string OrderDetails {get; set;}

        public Location PickUpLocation { get; set; }
        public Location DropOffLocation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}