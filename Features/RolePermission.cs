public class RolePermission
{
    public int Id { get; set; }
    public string OrgRole { get; set; } // Owner/Admin/Support/Rider
    public int PermissionId { get; set; }
}