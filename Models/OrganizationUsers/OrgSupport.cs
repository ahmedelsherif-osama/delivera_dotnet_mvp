namespace Delivera.Models
{
    class OrgSupport : BaseUser
    {
        public OrgSupport()
        {
            GlobalRole = GlobalRole.OrgSupport;
            OrganizationRole = OrganizationRole.OrgSupport;
        }
    }
}