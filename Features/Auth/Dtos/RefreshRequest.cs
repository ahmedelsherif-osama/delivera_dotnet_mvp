using System.ComponentModel.DataAnnotations;

namespace Delivera.DTOs
{
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}