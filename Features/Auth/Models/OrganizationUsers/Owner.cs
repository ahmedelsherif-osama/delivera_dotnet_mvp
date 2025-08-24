namespace Delivera.Models
{
    public class OrgOwner : BaseUser
    {
        public OrgOwner()
        {
            GlobalRole = GlobalRole.OrgUser;
            this.OrganizationRole = Delivera.Models.OrganizationRole.Owner;
        }
    }
}