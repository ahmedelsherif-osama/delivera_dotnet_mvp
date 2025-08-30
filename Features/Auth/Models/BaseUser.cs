namespace Delivera.Models
{
    public class BaseUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string NationalId { get; set; } = string.Empty;
        public bool IsOrgOwnerApproved { get; set; } = false;
        public bool IsSuperAdminApproved { get; set; } = false;

        public bool IsActive => IsOrgOwnerApproved && IsSuperAdminApproved;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
        public GlobalRole GlobalRole { get; set; }
        public OrganizationRole? OrganizationRole { get; set; }


        public Guid? CreatedById { get; set; }
        public BaseUser? CreatedByUser { get; set; }

        public Guid? ApprovedById { get; set; }
        public BaseUser? ApprovedByUser { get; set; }


        public Guid? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

    }
}