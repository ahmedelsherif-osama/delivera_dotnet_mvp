namespace Delivera.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrganizationId { get; set; }
        public Guid? RiderId { get; set; }
        public OrderStatus Status { get; set; }

        public Location PickUpLocation { get; set; }
        public Location DropOffLocation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string OrderDetails { get; set; } = null!;

        public Guid? RiderSessionId { get; set; }
        public RiderSession? RiderSession { get; set; }

        public Guid CreatedById { get; set; }
        public BaseUser CreatedByUser { get; set; }
    }

    public class Location
    {
        public string Address { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public enum OrderStatus
    {
        Created,
        Assigned,
        PickedUp,
        Delivered,
        Canceled
    }

}
