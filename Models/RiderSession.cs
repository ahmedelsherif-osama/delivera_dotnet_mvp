namespace Delivera.Models
{
    public class RiderSession
    {
        public Guid RiderId { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Zone { get; set; } = string.Empty;
        public RiderStatus Status { get; set; } = RiderStatus.Active;
        public List<Guid> ActiveOrders { get; set; } = new();
    }
}