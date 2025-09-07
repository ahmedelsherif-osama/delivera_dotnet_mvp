// namespace Delivera.Models
// {
//     public class RiderState
//     {
//         public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid RiderId { get; set; }
//         public Rider Rider { get; set; } = null!;

//         public double Latitude { get; set; }
//         public double Longitude { get; set; }

//         public RiderStatus Status { get; set; } = RiderStatus.LoggedOut;

//         public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
//     }

//     public enum RiderStatus
//     {
//         Active,
//         OnBreak,
//         LoggedOut
//     }
// }