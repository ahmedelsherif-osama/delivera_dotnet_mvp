namespace Delivera.Models
{
    public class RiderSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid? ZoneId { get; set; }
        public Zone? Zone { get; set; }

        public Guid RiderId { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Active;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;



        public List<Guid> ActiveOrders { get; set; } = new();
    }
    public enum SessionStatus
    {
        Active,
        OnBreak,
        Completed
    }
}

