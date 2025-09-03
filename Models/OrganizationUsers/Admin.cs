namespace Delivera.Models
{
    class OrgAdmin : BaseUser
    {
        public OrgAdmin()
        {
            GlobalRole = GlobalRole.SuperAdmin;
            this.OrganizationRole = Models.OrganizationRole.Admin;

        }
    }
}