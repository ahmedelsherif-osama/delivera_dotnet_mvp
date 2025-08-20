namespace Delivera.Models
{
    public class OrgOwner : BaseUser
    {
        public OrgOwner()
        {
            GlobalRole = GlobalRole.OrgUser;
            OrganizationRole = OrganizationRole.Owner;
        }
    }
}