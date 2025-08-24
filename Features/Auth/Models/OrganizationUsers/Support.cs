namespace Delivera.Models
{
    class OrgSupport : BaseUser
    {
        public OrgSupport()
        {
            GlobalRole = GlobalRole.OrgUser;
            this.OrganizationRole = Models.OrganizationRole.Support;
        }
    }
}