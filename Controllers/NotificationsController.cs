using System.Security.Claims;
using Delivera.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivera.Controllers;

[ApiController]
[Route("api/[controller]")]

public class NotificationsController : ControllerBase
{

    // private readonly IConfiguration _config;
    private readonly DeliveraDbContext _context;

    private readonly INotificationService _notificationService;


    public NotificationsController(DeliveraDbContext context, IConfiguration config, INotificationService notificationService)
    {
        _context = context;
        // _config = config;
        _notificationService = notificationService;
    }
    [HttpPut("markasread/{notificationId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> MarkNotificationAsRead(Guid notificationId)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
        if (notification == null) return BadRequest("Notification does not exist!");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return BadRequest("User does not exist!");
        if (Guid.Parse(userId) != notification.UserId) return Unauthorized();

        if (notification.Read) return BadRequest("Notification already marked as read!");

        notification.Read = true;
        await _context.SaveChangesAsync();

        return Ok("Notification marked as read!");

    }
}