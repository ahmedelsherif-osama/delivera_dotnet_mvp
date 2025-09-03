using NetTopologySuite.Geometries;

namespace Delivera.DTOs
{
    public class CreateZoneRequest 
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;

        // Polygon representing the zone
        public string WktPolygon { get; set; } = null!; //= null!; WKT string for the zone area
        
    }
}