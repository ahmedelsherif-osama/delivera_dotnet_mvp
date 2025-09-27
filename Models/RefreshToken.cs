namespace Delivera.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public BaseUser User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime RevokedAt { get; set; }
        public bool IsRevoked { get; set; } = false;
    }
}