namespace Delivera.Models
{
    public class RolePermission
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = null!; // just a name
        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}