public class Location
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Address { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}