namespace Delivera.DTOs
{
    public class RegisterResponse
    {

        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public GlobalRole Role { get; set; }
        public bool IsActive { get; set; }


    }
}