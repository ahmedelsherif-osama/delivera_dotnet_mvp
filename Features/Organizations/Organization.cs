namespace Delivera.Models
{
    public class Organization
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = !null;
        public string RegistrationNumber { get; set; } = !null;

        public bool IsApproved { get; set; } = false;

        public Guid OwnerId { get; set; } = !null;
        public OrgOwnerg Owner { get; set; }

        public List<BaseUser> Users { get; set; } = new();


    }
}