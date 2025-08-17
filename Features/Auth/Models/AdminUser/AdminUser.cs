namespace Delivera.Models
{
    public class AdminUser : BaseUser
    {
        public AdminUser()
        {
            globalRole = GlobalRole.SuperAdmin;
        }
    }
}