using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    // Optional: methods for clients to join groups
    public async Task JoinUserGroup(Guid userId)
    {
        Console.WriteLine("within hoinusergroup in hub");
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
    }

    public async Task LeaveUserGroup(Guid userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
    }
}
