using Delivera.Data;
using Delivera.Models;
using Microsoft.EntityFrameworkCore;

public interface INotificationService
{
    Task NotifySuperAdminAsync(string message);
    Task NotifyOrganizationOwnerAsync(Guid organizationId, string message);
    Task NotifyOrganizationAdminAsync(Guid organizationId, string message);
    Task NotifyOrderCreatorAsync(Order order, string message);
    Task NotifyRiderAsync(Guid riderId, string message);
    Task NotifyUserAsync(Guid userId, string message);
    Task NotifyOrgSupportAsync(Guid userOrgId, string message);
}


public class NotificationService : INotificationService
{
    private readonly DeliveraDbContext _context;

    public NotificationService(DeliveraDbContext context)
    {
        _context = context;
    }

    public async Task NotifyUserAsync(Guid userId, string message)
    {
        Console.WriteLine("heh" + userId);
        var notification = new Notification
        {
            UserId = userId,
            Message = message

        };
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }
    ///
    /// 
    public async Task NotifyOrgSupportAsync(Guid organizationId, string message)
    {
        var orgSupportUsers = await _context.Users
        .AsNoTracking()
        .Where(u => u.OrganizationId == organizationId && u.OrganizationRole == OrganizationRole.Support)
        .ToListAsync();

        foreach (var user in orgSupportUsers)
        {
            await NotifyUserAsync(user.Id, message);
        }

    }

    public async Task NotifyOrganizationOwnerAsync(Guid organizationId, string message)
    {
        var orgowner = await _context.Users.FirstAsync(u => u.OrganizationRole == OrganizationRole.Owner && u.OrganizationId == organizationId);
        await NotifyUserAsync(orgowner.Id, message);

    }








    public async Task NotifySuperAdminAsync(string message)
    {
        var superadmin = await _context.Users.FirstOrDefaultAsync(u => u.GlobalRole == GlobalRole.SuperAdmin);
        await NotifyUserAsync(superadmin.Id, message);

    }
    public async Task NotifyOrganizationAdminAsync(Guid organizationId, string message)
    {
        var orgAdmin = await _context.Users.FirstOrDefaultAsync(u => u.OrganizationRole == OrganizationRole.Admin && u.OrganizationId == organizationId);
        await NotifyUserAsync(orgAdmin.Id, message);

    }

    public async Task NotifyOrderCreatorAsync(Order order, string message)
    {
        var createdById = order.CreatedById;
        await NotifyUserAsync(createdById, message);
    }
    public async Task NotifyRiderAsync(Guid riderId, string message)
    {
        await NotifyUserAsync(riderId, message);

    }



}