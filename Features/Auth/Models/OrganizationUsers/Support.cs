namespace Delivera.Models
{
    class OrgSupport : BaseUser
    {
        public OrgSupport()
        {
            GlobalRole = GlobalRole.OrgUser;
            OrganizationRole = OrganizationRole.Support;
        }
    }
}