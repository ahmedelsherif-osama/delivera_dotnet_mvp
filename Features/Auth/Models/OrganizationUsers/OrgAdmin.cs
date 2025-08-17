namespace Delivera.Models
{
    class OrgAdmin : BaseUser
    {
        public OrgAdmin()
        {
            GlobalRole = GlobalRole.SuperAdmin;
            OrganizationRole = OrganizationRole.OrgAdmin;

        }
    }
}