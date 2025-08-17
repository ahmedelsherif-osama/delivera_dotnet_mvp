namespace Delivera.Models
{
    class Rider : BaseUser
    {
        public Rider()
        {
            GlobalRole = GlobalRole.OrgUser;
            OrganizationRole = OrganizationRole.Rider;
        }
    }
}