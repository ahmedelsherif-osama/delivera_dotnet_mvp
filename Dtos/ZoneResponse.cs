
namespace Delivera.DTOs
{    
    public class ZoneResponse
        {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string WktPolygon { get; set; } = null!;
    }
}