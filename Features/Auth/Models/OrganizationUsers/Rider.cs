namespace Delivera.Models
{
    class Rider : BaseUser
    {
        public Rider()
        {
            GlobalRole = GlobalRole.OrgUser;
            this.OrganizationRole = Models.OrganizationRole.Rider;
        }
    }
}