namespace Delivera.Models
{
    class OrgOwner : BaseUser
    {
        public OrgOwner()
        {
            GlobalRole = GlobalRole.OrgUser;
            OrganizationRole = OrganizationRole.OrgOwner;
        }
    }
}