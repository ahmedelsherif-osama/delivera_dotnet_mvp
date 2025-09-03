namespace Delivera.Models
{
    public class AdminUser : BaseUser
    {
        public AdminUser()
        {
            GlobalRole = GlobalRole.SuperAdmin;
        }
    }
}