public class Order
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? RiderId { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}