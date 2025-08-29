using System.ComponentModel.DataAnnotations;
using Delivera.Models;
namespace Delivera.DTOs
{
    public class RegisterRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(20)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*#?&]).+$",
       ErrorMessage = "Password must contain upper, lower, number, and special char.")]
        public string Password { get; set; } = string.Empty;


        [Required]
        [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Phone number must be 10–15 digits and may start with +.")]
        public string PhoneNumber { get; set; } = string.Empty;


        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(RegisterRequest), nameof(ValidateDateOfBirth))]
        public DateTime? DateOfBirth { get; set; }



        [Required]
        public string NationalId { get; set; } = string.Empty;
        [Required]
        public bool IsActive { get; set; } = true;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public GlobalRole GlobalRole { get; set; }

        public OrganizationRole? OrganizationRole { get; set; }

        public Guid? CreatedById { get; set; }
        public Guid? ApprovedById { get; set; }
        // public BaseUser? CreatedByUser { get; set; }

        public Guid? OrganizationId { get; set; }
        // public Organization? Organization { get; set; }

        public static ValidationResult? ValidateDateOfBirth(DateTime? dob, ValidationContext context)
        {
            if (dob == null)
                return new ValidationResult("Date of Birth is required.");

            if (dob > DateTime.UtcNow)
                return new ValidationResult("Date of Birth cannot be in the future.");

            var age = DateTime.UtcNow.Year - dob.Value.Year;
            if (dob.Value.Date > DateTime.UtcNow.AddYears(-age)) age--; // adjust if birthday not reached yet

            if (age < 18)
                return new ValidationResult("User must be at least 18 years old.");

            return ValidationResult.Success;
        }

    }
}