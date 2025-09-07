using NetTopologySuite.Geometries;

namespace Delivera.Models
{
    public class Zone
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;

        // Polygon representing the zone
        public string WktPolygon { get; set; } = null!; // WKT string for the zone area
        public Geometry Area { get; set; } = null!; // <-- Add this
        
        public Guid OrganizationId {get; set;}
        public Organization Org {get; set;}
        }
}
