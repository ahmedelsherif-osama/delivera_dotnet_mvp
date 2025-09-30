using Delivera.Helpers;

namespace Delivera.Models
{
    public class Organization
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ShortId => CodeGeneratorHelper.Base62Encode(Id);
        public string Name { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = false;

        public Guid OwnerId { get; set; } = Guid.NewGuid();
        public OrgOwner Owner { get; set; }

        public List<BaseUser> Users { get; set; } = new();


    }
}