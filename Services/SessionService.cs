using System.Collections.Concurrent;
using Delivera.Models;

namespace Delivera.Services
{
    public class SessionManager
    {
        private readonly ConcurrentDictionary<Guid, RiderSession> _sessions = new();

        public void StartSession(Guid riderId, double latitude, double longitude, string zone)
        {
            var session = new RiderSession
            {
                RiderId = riderId,
                Latitude = latitude,
                Longitude = longitude,
                Zone = zone,
                Status = RiderStatus.Active
            };
            _sessions[riderId] = session;
        }

        public void UpdateLocation(Guid riderId, double latitude, double longitude, string zone)
        {
            if (_sessions.TryGetValue(riderId, out var session))
            {
                session.Latitude = latitude;
                session.Longitude = longitude;
                session.Zone = zone;
            }
        }

        public void UpdateStatus(Guid riderId, RiderStatus status)
        {
            if (_sessions.TryGetValue(riderId, out var session))
            {
                session.Status = status;
            }
        }

        public void EndSession(Guid riderId)
        {
            _sessions.TryRemove(riderId, out _);
        }

        public RiderSession? GetNearestActiveRider(double latitude, double longitude, string zone)
        {
            var candidates = _sessions.Values
                .Where(s => s.Zone == zone && s.Status == RiderStatus.Active);

            return candidates
                .OrderBy(s => Distance(latitude, longitude, s.Latitude, s.Longitude))
                .FirstOrDefault();
        }

        private double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversine formula
            double R = 6371e3; // earth radius in meters
            double φ1 = lat1 * Math.PI / 180;
            double φ2 = lat2 * Math.PI / 180;
            double Δφ = (lat2 - lat1) * Math.PI / 180;
            double Δλ = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                       Math.Cos(φ1) * Math.Cos(φ2) *
                       Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }
}
