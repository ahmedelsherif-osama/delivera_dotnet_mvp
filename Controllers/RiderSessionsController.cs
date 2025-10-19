using System.Security.Claims;
using System.Text.Json;
using Deliver.DTOs;
using Delivera.Data;
using Delivera.Models;
using Humanizer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Delivera.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RiderSessionsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly DeliveraDbContext _context;

    private readonly INotificationService _notificationService;
    public RiderSessionsController(DeliveraDbContext context, IConfiguration config, INotificationService notificationService)
    {
        _context = context;
        _config = config;
        _notificationService = notificationService;

    }

    [HttpGet("all")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetOrganizationsActiveRiderSessions()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == OrganizationRole.Rider.ToString())
        {
            return Unauthorized();
        }

        var userOrgId = User.FindFirstValue("OrgId");
        var sessions = await _context.RiderSessions.Where((s) => s.Status == SessionStatus.Active && s.OrganizationId == Guid.Parse(userOrgId!)).ToListAsync();

        if (sessions.IsNullOrEmpty())
        {
            return Ok("No active sessions for your organization!");
        }

        return Ok(sessions);
    }


    [HttpPut("endridersession/{sessionId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public async Task<IActionResult> EndRiderSession(Guid sessionId)
    {
        Console.WriteLine("within rider session controller");
        var session = await _context.RiderSessions.FirstAsync<RiderSession>(rs => rs.Id == sessionId);
        var userOrgId = User.FindFirstValue("OrgId");
        var sessionRiderId = session.RiderId;
        var rider = await _context.Users.FirstAsync(u => u.Id == sessionRiderId);
        var sessionOrgId = rider.OrganizationId;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);


        if (session == null) return BadRequest("Invalid Session Id");
        if (userOrgId == null || userOrgId != sessionOrgId.ToString()) return Unauthorized();
        if (role == OrganizationRole.Rider.ToString() && sessionRiderId.ToString() != userId) return Unauthorized();
        if (session.Status == SessionStatus.Completed) return BadRequest("Session already completed!");

        session.Status = SessionStatus.Completed;
        session.LastUpdated = DateTime.Now;
        var totalSessionDuration = session.LastUpdated - session.StartedAt;
        var totalHours = totalSessionDuration.Hours;

        // Example: total duration in minutes
        var totalMinutes = totalSessionDuration.TotalMinutes;

        // Example: total duration in seconds
        var totalSeconds = totalSessionDuration.TotalSeconds;

        await _context.SaveChangesAsync();
        var message = $"Session #{sessionId.ToString().Substring(0, 8)} for rider {rider.FirstName + " " + rider.LastName} is completed at {session.LastUpdated.Humanize()}  total duration is {totalHours} hours";

        await _notificationService.NotifyOrganizationAdminAsync(Guid.Parse(userOrgId), message);
        await _notificationService.NotifyRiderAsync(rider.Id, message);
        await _notificationService.NotifyOrgSupportAsync(Guid.Parse(userOrgId), message);
        return Ok("Session successfully completed!" + session);



    }

    public record UpdateLocationRequest(double Latitude, double Longitude);

    [HttpPut("updateriderlocation")]
    public IActionResult UpdateLocation([FromBody] UpdateLocationRequest req)
    {
        var riderIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine(riderIdStr);
        if (string.IsNullOrEmpty(riderIdStr))
            return Unauthorized("Rider ID missing in token");

        var riderId = Guid.Parse(riderIdStr);

        var session = _context.RiderSessions.FirstOrDefault(s => s.RiderId == riderId && s.Status != SessionStatus.Completed);

        if (session == null)
        {
            return BadRequest("No active session for this rider!");
        }

        var point = new NetTopologySuite.Geometries.Point(req.Longitude, req.Latitude);

        var zone = _context.Zones
    .AsEnumerable() // forces evaluation in memory
    .FirstOrDefault(z => z.Area.Contains(point));



        if (zone == null)
        {
            return BadRequest(new { message = "Rider out of zone!" });
        }

        session.Latitude = req.Latitude;
        session.Longitude = req.Longitude;
        session.ZoneId = zone.Id;
        session.LastUpdated = DateTime.Now;

        _context.SaveChanges();

        return Ok(new { message = "Location updated", riderId });
    }

    public RiderSession? GetNearestActiveRider(double latitude, double longitude, Guid zoneId)
    {
        var sessions = _context.RiderSessions.Where(s => s.ZoneId == zoneId && s.Status == SessionStatus.Active).ToList<RiderSession>();
        sessions.ForEach((s) => Console.WriteLine(s.Id + " here we go"));
        Console.WriteLine(sessions.IsNullOrEmpty());
        if (sessions.IsNullOrEmpty())
        {
            Console.WriteLine("no sessions");
            return null;
        }
        return sessions
            .OrderBy(s => Distance(latitude, longitude, s.Latitude, s.Longitude))
            .FirstOrDefault();
    }

    private double Distance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula
        double R = 6371e3; // earth radius in meters
        double φ1 = lat1 * Math.PI / 180;
        double φ2 = lat2 * Math.PI / 180;
        double Δφ = (lat2 - lat1) * Math.PI / 180;
        double Δλ = (lon2 - lon1) * Math.PI / 180;

        double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                   Math.Cos(φ1) * Math.Cos(φ2) *
                   Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    [HttpPatch("assignrider")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public async Task<ActionResult> AssignRider(Guid orderId)
    {
        Console.WriteLine("within assign rider");
        // ✅ 1. Find order
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        Console.WriteLine("found order");
        if (order == null) return NotFound(new { error = "Order not found" });
        if (order.PickUpLocation == null)
            return BadRequest("Order has no pickup location");
        if (order.Status != OrderStatus.Created)
        {

            return BadRequest(new { message = $"Order already {order.Status.ToString().Replace("OrderStatus", "")}!" });
        }

        // ✅ 2. Check user permissions
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == OrganizationRole.Rider.ToString())
        {
            return Forbid("Only admins or owners can assign riders");
        }

        Console.WriteLine("done with finding orders and permissions");


        // ✅ 3. Determine zone (MVP = naive approach)
        var pickupPoint = new NetTopologySuite.Geometries.Point(
               order.PickUpLocation.Longitude, // X
    order.PickUpLocation.Latitude   // Y
        );

        var zones = await _context.Zones.ToListAsync();
        Console.WriteLine(pickupPoint);
        Console.WriteLine("Pickup point fine");


        zones.ForEach(z =>
        {
            Console.WriteLine($"${z.Area} ${z.Id} ${z.WktPolygon}");
        });

        var zone = zones.FirstOrDefault(z => z.Area != null && z.Area.Contains(pickupPoint));
        Console.WriteLine("Zone found 1");

        if (zone == null)
        {
            Console.WriteLine("No Zone");

            return NotFound(new { message = "Pickup location not in any zone" });
        }
        Console.WriteLine("Zone found");
        // ...
        Console.WriteLine(JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Zone: {zone?.Name}, Polygon: {zone?.WktPolygon}");
        Console.WriteLine(role);
        Console.WriteLine(order.PickUpLocation.Latitude);
        Console.WriteLine(order.PickUpLocation.Longitude);
        Console.WriteLine(zone!.Name);
        // ✅ 4. Find nearest rider in that zone
        var riderSession = GetNearestActiveRider(
            order.PickUpLocation.Latitude,
            order.PickUpLocation.Longitude,
            zone.Id
        );

        Console.WriteLine($"rider {riderSession}");

        if (riderSession == null)
        {
            return NotFound(new { message = "No active rider available in this zone" });
        }



        // ✅ 5. Update order + rider
        order.Status = OrderStatus.Assigned;
        order.RiderId = riderSession.RiderId; // 🔴 for MVP only, better to store RiderId as Guid
        order.RiderSessionId = riderSession.Id;
        riderSession.CurrentOrderId = order.Id;
        riderSession.CurrentOrderPickUp = order.PickUpLocation;
        riderSession.CurrentOrderDropOff = order.DropOffLocation;

        _context.SaveChanges();

        var message = $"Order #{order.Id.ToString().Substring(0, 8)} assigned to rider #{riderSession.Id.ToString().Substring(0, 8)}";
        await _notificationService.NotifyRiderAsync(riderSession.RiderId, message);
        await _notificationService.NotifyOrderCreatorAsync(order, message);

        return Ok(new
        {
            message = "Rider assigned",
            riderId = riderSession.RiderId
        });
    }

    [Authorize]
    [HttpPatch("admin/assignrider")]
    public async Task<ActionResult> AssignRiderAdmin(Guid orderId, [FromBody] AssignRiderRequest assignRiderRequest)
    {
        Console.WriteLine("within assign rider");
        // ✅ 1. Find order
        var order = await _context.Orders.Include(o => o.CreatedByUser).FirstOrDefaultAsync(o => o.Id == orderId);
        Console.WriteLine("found order");
        if (order == null) return NotFound(new { error = "Order not found" });
        if (order.PickUpLocation == null)
            return BadRequest("Order has no pickup location");

        var orgId = User.FindFirstValue("OrgId");
        if (orgId == null) return Unauthorized("You do not belong to any organization!");

        var orderCreator = order.CreatedByUser;
        if (orderCreator == null) return NotFound("Order creator not found!");

        if (Guid.Parse(orgId) != orderCreator.OrganizationId) return Unauthorized("You do not belong to this Organization!");

        // ✅ 2. Check user permissions
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != OrganizationRole.Owner.ToString() &&
            role != OrganizationRole.Admin.ToString())
        {
            return Forbid("Only admins or owners can assign riders");
        }

        Console.WriteLine("done with finding orders and permissions");
        var riderId = assignRiderRequest.RiderId;


        var rider = await _context.Users.FirstOrDefaultAsync(u => u.Id == riderId);
        if (rider == null) return BadRequest(new { message = "Rider ID not found, contact admin!" });


        if (Guid.Parse(orgId) != rider.OrganizationId) return Unauthorized("This rider does not belong to your organization!");

        var riderSession = await _context.RiderSessions.FirstOrDefaultAsync(s => s.RiderId == riderId && s.Status != SessionStatus.Completed);
        if (riderSession == null) return BadRequest("Rider is not logged in! Please ask the rider to login first!");

        riderSession.CurrentOrderId = orderId;
        riderSession.CurrentOrderPickUp = order.PickUpLocation;
        riderSession.CurrentOrderDropOff = order.DropOffLocation;

        order.RiderId = riderId;
        order.RiderSessionId = riderSession.Id;
        order.RiderSession = riderSession;
        order.Status = OrderStatus.Assigned;


        await _context.SaveChangesAsync();

        var message = $"Order #{order.Id.ToString().Substring(0, 8)} assigned to rider #{riderId.ToString().Substring(0, 8)}";
        await _notificationService.NotifyRiderAsync(riderId, message);
        await _notificationService.NotifyOrderCreatorAsync(order, message);

        return Ok(new
        {
            message = "Rider assigned",
            riderId = rider.Id
        });
    }

    [HttpGet("admin/riders")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> GetAllRiders()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == OrganizationRole.Rider.ToString() || role == OrganizationRole.Support.ToString())
        {
            return Unauthorized();
        }
        var userOrgId = User.FindFirstValue("OrgId");
        if (userOrgId.IsNullOrEmpty())
        {
            return BadRequest(new { message = "You do not belong to any organization! Contact admin." });
        }
        var sessions = await _context.RiderSessions.Where(s => s.OrganizationId == Guid.Parse(userOrgId!) && s.Status != SessionStatus.Completed).ToListAsync();
        if (sessions.IsNullOrEmpty())
        {
            sessions = new List<RiderSession> { };
        }
        var activeSessions = sessions.Where(s => s.Status == SessionStatus.Active);                // eager load orders;

        var onBreakSessions = sessions.Where(s => s.Status == SessionStatus.OnBreak);
        var riders = await _context.Users.Where(u => u.OrganizationRole == OrganizationRole.Rider && u.OrganizationId == Guid.Parse(userOrgId!)).ToListAsync();
        if (riders.IsNullOrEmpty())
        {
            riders = new List<BaseUser> { };
        }
        var inSessionRidersIds = sessions.Select(s => s.RiderId).Distinct().ToHashSet();
        var offlineRiders = riders.Where(r => !inSessionRidersIds.Contains(r.Id)).ToList();

        return Ok(
            new
            {
                activeRiderSessions = activeSessions,
                onBreakRiderSessions = onBreakSessions,
                offlineRiders = offlineRiders

            }
        );




    }


}