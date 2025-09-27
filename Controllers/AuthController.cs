using Microsoft.AspNetCore.Mvc;
using Delivera.Models;
using Delivera.DTOs;
using Delivera.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Drawing.Printing;
using System.Text.Json;
using NetTopologySuite.Utilities;
using Microsoft.Identity.Client;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Http.HttpResults;
using Humanizer;
using Deliver.DTOs;

namespace Delivera.Controllers;


[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly DeliveraDbContext _context;

    private readonly INotificationService _notificationService;


    public AuthController(DeliveraDbContext context, IConfiguration config, INotificationService notificationService)
    {
        _context = context;
        _config = config;
        _notificationService = notificationService;
    }

    [HttpPut("notifications/markasread/{notificationId}")]
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

    [HttpGet("orders/")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetAllOrgOrders()
    {
        var orgId = User.FindFirstValue("OrgId");
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == OrganizationRole.Rider.ToString())
        {
            return Unauthorized("Access Denied!");
        }
        if (!Guid.TryParse(orgId, out var orgGuid))
        {
            return BadRequest("Organization Id is in wrong format!");
        }
        var orders = await _context.Orders.Where(o => o.OrganizationId == orgGuid).ToListAsync();

        if (orders.IsNullOrEmpty())
        {
            return Ok("No orders yet!");
        }

        return Ok(orders);
    }

    [HttpPut("endridersession/{sessionId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public async Task<IActionResult> EndRiderSession(Guid sessionId)
    {
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
        var message = $"Session #{sessionId} for rider {rider.FirstName + " " + rider.LastName} is completed at {session.LastUpdated.Humanize()}  total duration is {totalHours} hours";

        await _notificationService.NotifyOrganizationAdminAsync(Guid.Parse(userOrgId), message);
        await _notificationService.NotifyRiderAsync(rider.Id, message);
        await _notificationService.NotifyOrgSupportAsync(Guid.Parse(userOrgId), message);
        return Ok("Session successfully completed!" + session);



    }


    [HttpGet("orders/superadmin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetAllOrdersAdmin()
    {
        var role = User.FindFirstValue("Role");
        if (role != GlobalRole.SuperAdmin.ToString())
        {
            return Unauthorized("Access Denied");
        }
        var orders = await _context.Orders.ToListAsync();
        return Ok(orders);
    }

    [HttpGet("orders/own")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetOwnOrders()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != OrganizationRole.Rider.ToString())
        {
            return Unauthorized("This endpoint is for riders only!");
        }
        var riderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine("orders own rider id " + riderId);


        var orders = _context.Orders.Where(o => o.RiderId.ToString()!.ToUpper() == riderId!.ToUpper());

        if (orders.IsNullOrEmpty())
        {
            return Ok("You have no orders yet!");

        }
        return Ok(orders);

    }


    [HttpGet("orders/{orderId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetOrder(Guid orderId)
    {
        var orgId = User.FindFirstValue("OrgId");
        var order = await _context.Orders.FirstOrDefaultAsync<Order>(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound("Order not found!");
        }

        if (orgId != order.OrganizationId.ToString())
        {
            return Unauthorized();
        }


        return Ok(order);

    }

    [Authorize]
    [HttpGet("organizations")]
    public async Task<IActionResult> GetOrganizations()
    {
        var organizations = await _context.Organizations.ToListAsync();
        if (organizations.IsNullOrEmpty())
        {
            return NotFound("No organizations were found!");
        }
        return Ok(organizations);
    }

    [HttpPatch("updateOrderStatus")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest updateStatusRequest)
    {
        var order = await _context.Orders.FirstOrDefaultAsync<Order>(o => o.Id == updateStatusRequest.OrderId);
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (order == null)
        {
            return NotFound("Order not found!");
        }
        if (updateStatusRequest.Status == OrderStatus.Canceled || updateStatusRequest.Status == OrderStatus.Canceled)
        {
            if (role == OrganizationRole.Rider.ToString())
            {
                return Unauthorized("Riders cannot remove orders, please contact support!");
            }
        }

        order.Status = updateStatusRequest.Status;
        var message = $"Order {order.Id} {updateStatusRequest.Status}";
        if (order.RiderId == null || order.RiderId.ToString() == "")
        {
            return BadRequest("Order must be assigned to a rider before status update");
        }

        await _notificationService.NotifyRiderAsync(order.RiderId.Value, message);
        await _notificationService.NotifyOrderCreatorAsync(order, message);

        await _context.SaveChangesAsync();



        return Ok(order);

    }


    // DTO for update requests
    public record UpdateLocationRequest(double Latitude, double Longitude);

    /// <summary>
    /// Rider updates location (creates session if not existing).
    /// </summary>
    [HttpPut("update")]
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
            return BadRequest("Rider our of zone!");
        }

        session.Latitude = req.Latitude;
        session.Longitude = req.Longitude;
        session.ZoneId = zone.Id;
        session.LastUpdated = DateTime.Now;

        _context.SaveChanges();

        return Ok(new { message = "Location updated", riderId });
    }



    [Authorize]
    [HttpGet("getzone/{id}")]
    public async Task<IActionResult> GetZone(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var orgId = User.FindFirstValue("OrgId");


        if (string.IsNullOrEmpty(userId)) return Unauthorized("Invalid token");

        if (role == OrganizationRole.Rider.ToString() || role == OrganizationRole.Support.ToString())
            return Unauthorized("This user type cannot access zones");

        // if (orgId != orderRequest.OrganizationId.ToString())
        //     return Unauthorized("User does not belong to this organization");
        var zone = await _context.Zones.FindAsync(id);
        if (zone == null) return NotFound();
        var response = new ZoneResponse
        {
            Id = zone.Id,
            Name = zone.Name,
            WktPolygon = zone.Area.AsText() // Convert Geometry to WKT string

        };

        return Ok(response);
    }


    [HttpPatch("superadmin/approveOrg/{organizationId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ApproveOrganization(Guid organizationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue("Role");
        // var role2 = User.FindFirstValue(ClaimTypes.Role);

        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }
        Console.WriteLine($"role 1: ${role} - role 2: $");
        if (role != GlobalRole.SuperAdmin.ToString())
        {
            return Unauthorized("Only administrator can approve an organization!");
        }

        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId);
        if (org == null)
        {
            return NotFound("Organization not found!");
        }

        org.IsApproved = true;

        await _context.SaveChangesAsync();

        var message = $"Organization #{org.Id} {org.Name} is now approved";
        await _notificationService.NotifyOrganizationOwnerAsync(org.Id, message);
        await _notificationService.NotifySuperAdminAsync(message);


        return Ok("Organization approved successfully!");

    }


    [Authorize]
    [HttpPost("zones")]
    public async Task<IActionResult> CreateZone([FromBody] CreateZoneRequest dto)
    {
        //token check
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var orgId = User.FindFirstValue("OrgId");
        if (string.IsNullOrEmpty(orgId)) return BadRequest("OrganizationId missing");
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == Guid.Parse(orgId));
        if (org == null) return BadRequest("You don't belong to any organization!");



        if (string.IsNullOrEmpty(userId)) return Unauthorized("Invalid token");

        if (role == OrganizationRole.Rider.ToString() || role == OrganizationRole.Support.ToString())
            return Unauthorized("This user type cannot create zones");

        // if (orgId != orderRequest.OrganizationId.ToString())
        //     return Unauthorized("User does not belong to this organization");

        var wktReader = new WKTReader();
        Geometry area;

        try
        {
            area = wktReader.Read(dto.WktPolygon);
            if (!(area is Polygon || area is MultiPolygon))
            {
                return BadRequest("The WKT must represent a Polygon or MultiPolygon.");
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Invalid WKT format: {ex.Message}");
        }

        var zone = new Zone
        {
            Name = dto.Name,
            WktPolygon = dto.WktPolygon,
            Area = area,
            OrganizationId = Guid.Parse(orgId)
        };

        _context.Zones.Add(zone);
        await _context.SaveChangesAsync();
        var response = new ZoneResponse
        {
            Id = zone.Id,
            Name = zone.Name,
            WktPolygon = zone.Area.AsText()
        };
        var message = $"New Zone #{zone.Id} {zone.Name} created for Organization #{orgId} {org.Name}";
        await _notificationService.NotifyOrganizationOwnerAsync(Guid.Parse(orgId), message);
        await _notificationService.NotifyOrganizationAdminAsync(Guid.Parse(orgId), message);

        return CreatedAtAction(nameof(GetZone), new { id = zone.Id }, response);
    }


    public RiderSession? GetNearestActiveRider(double latitude, double longitude, Guid zoneId)
    {
        var sessions = _context.RiderSessions.Where(s => s.ZoneId == zoneId).ToList<RiderSession>();

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


    [Authorize]
    [HttpPut("assignRider")]
    public async Task<ActionResult> AssignRider(Guid orderId)
    {
        Console.WriteLine("within assign rider");
        // ✅ 1. Find order
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        Console.WriteLine("found order");
        if (order == null) return NotFound(new { error = "Order not found" });
        if (order.PickUpLocation == null)
            return BadRequest("Order has no pickup location");

        // ✅ 2. Check user permissions
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != OrganizationRole.Owner.ToString() &&
            role != OrganizationRole.Admin.ToString())
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
            return NotFound(new { error = "Pickup location not in any zone" });
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
        var rider = GetNearestActiveRider(
            order.PickUpLocation.Latitude,
            order.PickUpLocation.Longitude,
            zone.Id
        );

        if (rider == null)
            return Ok(new { message = "No active rider available in this zone" });

        // ✅ 5. Update order + rider
        order.Status = OrderStatus.Assigned;
        order.RiderId = rider.RiderId; // 🔴 for MVP only, better to store RiderId as Guid
        rider.ActiveOrders.Add(order);

        _context.SaveChanges();

        var message = $"Order #{order.Id} assigned to rider #{rider.Id}";
        await _notificationService.NotifyRiderAsync(rider.RiderId, message);
        await _notificationService.NotifyOrderCreatorAsync(order, message);

        return Ok(new
        {
            message = "Rider assigned",
            riderId = rider.RiderId
        });
    }

    [Authorize]
    [HttpPut("admin/assignrider")]
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
        if (rider == null) return BadRequest("Rider not found!");


        if (Guid.Parse(orgId) != rider.OrganizationId) return Unauthorized("This rider does not belong to your organization!");

        var riderSession = await _context.RiderSessions.FirstOrDefaultAsync(s => s.RiderId == riderId && s.Status != SessionStatus.Completed);
        if (riderSession == null) return BadRequest("Rider is not logged in! Please ask the rider to login first!");



        order.RiderId = riderId;
        order.RiderSessionId = riderSession.Id;
        order.RiderSession = riderSession;

        await _context.SaveChangesAsync();

        var message = $"Order #{order.Id} assigned to rider #{riderId}";
        await _notificationService.NotifyRiderAsync(riderId, message);
        await _notificationService.NotifyOrderCreatorAsync(order, message);

        return Ok(new
        {
            message = "Rider assigned",
            riderId = rider.Id
        });
    }


    [Authorize]
    [HttpPost("createOrder")]
    public async Task<ActionResult> CreateOrder([FromBody] OrderRequest orderRequest)
    {
        if (orderRequest.OrderDetails == null || orderRequest.OrderDetails == "")
        {
            return BadRequest("Order details are required!");
        }
        if (orderRequest.OrganizationId == null)
        {
            return BadRequest("Organization Id is required!");
        }
        if (orderRequest.PickUpLocation == null)
        {
            return BadRequest("Pickup location is required!");
        }
        if (orderRequest.DropOffLocation == null)
        {
            return BadRequest("DropOff location is required!");
        }

        //token check
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine($"what now {userId}");
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var orgId = User.FindFirstValue("OrgId");


        if (string.IsNullOrEmpty(userId)) return Unauthorized("Invalid token");

        if (role == OrganizationRole.Rider.ToString())
            return Unauthorized("Riders cannot create orders");

        if (orgId != orderRequest.OrganizationId.ToString())
            return Unauthorized("User does not belong to this organization");


        // Validate input
        if (string.IsNullOrWhiteSpace(orderRequest.OrderDetails))
            return BadRequest("Order details are required!");

        if (orderRequest.PickUpLocation == null)
            return BadRequest("Pickup location is required!");

        if (orderRequest.DropOffLocation == null)
            return BadRequest("DropOff location is required!");

        Order order = new Order
        {
            OrganizationId = orderRequest.OrganizationId,
            Status = OrderStatus.Created,
            PickUpLocation = orderRequest.PickUpLocation,
            DropOffLocation = orderRequest.DropOffLocation,
            OrderDetails = orderRequest.OrderDetails,
            CreatedById = Guid.Parse(userId),
            RiderId = null
        };
        _context.Orders.Add(order);

        try
        {
            await _context.SaveChangesAsync();
            var message = $"New order #{order.Id} created for Organization #{orgId}";
            await _notificationService.NotifyOrderCreatorAsync(order, message);
            await _notificationService.NotifyOrganizationOwnerAsync(Guid.Parse(orgId), message);
            await _notificationService.NotifyOrganizationAdminAsync(Guid.Parse(orgId), message);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"DB Update Error: {ex.InnerException?.Message}");
            throw;
        }

        return Ok(new { message = "Order created successfully!" });


    }

    public async Task NotifyOrganizationOwnerAsync(Guid organizationId, string message)
    {
        var orgowner = await _context.Users.FirstAsync(u => u.OrganizationRole == OrganizationRole.Owner && u.OrganizationId == organizationId);
        Console.WriteLine($"orgowner {orgowner}");
        var notification = new Notification
        {
            UserId = orgowner.Id,
            Message = message

        };
        Console.WriteLine($"orgowner notif {notification}");

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        BaseUser user = new BaseUser();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email already registered.");
        if (await _context.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
            return BadRequest("Phone number already registered.");
        if (await _context.Users.AnyAsync(u => u.NationalId == request.NationalId))
            return BadRequest("National Id already registered.");

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username is not available.");

        if (request.GlobalRole == GlobalRole.SuperAdmin)
        {
            bool superAdminExists = await _context.Users
                .AnyAsync(u => u.GlobalRole == GlobalRole.SuperAdmin);

            if (superAdminExists)
                return BadRequest("A SuperAdmin already exists.");
        }

        switch (request.GlobalRole)
        {
            case GlobalRole.SuperAdmin:
                user = new AdminUser
                {
                    Email = request.Email,
                    Username = request.Username,
                    PasswordHash = HashPassword(request.Password),
                    GlobalRole = GlobalRole.SuperAdmin,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    DateOfBirth = request.DateOfBirth,
                    NationalId = request.NationalId,


                };
                _context.Users.Add(user);
                break;

            case GlobalRole.OrgUser:
                if (!request.OrganizationRole.HasValue)
                    return BadRequest("Organization role is required for OrgUser.");

                switch (request.OrganizationRole.Value)
                {
                    case OrganizationRole.Owner:
                        user = new OrgOwner
                        {
                            Email = request.Email,
                            Username = request.Username,
                            PasswordHash = HashPassword(request.Password),
                            GlobalRole = GlobalRole.OrgUser,
                            PhoneNumber = request.PhoneNumber,
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            DateOfBirth = request.DateOfBirth,
                            NationalId = request.NationalId,
                            OrganizationRole = OrganizationRole.Owner,
                            IsOrgOwnerApproved = true,
                            CreatedById = request.CreatedById,
                            OrganizationId = request.OrganizationId

                        };

                        _context.Users.Add(user);
                        await _context.SaveChangesAsync(); // ✅ ensure user.Id exists

                        var org = new Organization
                        {
                            IsApproved = false,
                            OwnerId = user.Id // ✅ now safe
                        };

                        _context.Organizations.Add(org);
                        await _context.SaveChangesAsync();

                        // optional: link user → org for convenience
                        user.OrganizationId = org.Id;
                        await _context.SaveChangesAsync();
                        break;

                    case OrganizationRole.Admin:
                        if (!request.OrganizationId.HasValue)
                            return BadRequest("OrganizationId required for Admin.");
                        user = new OrgAdmin
                        {
                            Email = request.Email,
                            Username = request.Username,
                            PasswordHash = HashPassword(request.Password),
                            GlobalRole = GlobalRole.OrgUser,
                            OrganizationId = request.OrganizationId.Value,
                            PhoneNumber = request.PhoneNumber,
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            DateOfBirth = request.DateOfBirth,
                            NationalId = request.NationalId,
                            OrganizationRole = OrganizationRole.Admin,
                            CreatedById = request.CreatedById,

                        };
                        _context.Users.Add(user);
                        break;

                    case OrganizationRole.Support:
                        if (!request.OrganizationId.HasValue)
                            return BadRequest("OrganizationId required for Support.");
                        user = new OrgSupport
                        {
                            Email = request.Email,
                            Username = request.Username,
                            PasswordHash = HashPassword(request.Password),
                            GlobalRole = GlobalRole.OrgUser,
                            OrganizationId = request.OrganizationId.Value,
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            DateOfBirth = request.DateOfBirth,
                            NationalId = request.NationalId,
                            OrganizationRole = OrganizationRole.Support,
                            PhoneNumber = request.PhoneNumber,

                            CreatedById = request.CreatedById
                        };
                        _context.Users.Add(user);
                        break;

                    case OrganizationRole.Rider:
                        if (!request.OrganizationId.HasValue)
                            return BadRequest("OrganizationId required for Rider.");
                        user = new Rider
                        {
                            Email = request.Email,
                            Username = request.Username,
                            PasswordHash = HashPassword(request.Password),
                            GlobalRole = GlobalRole.OrgUser,
                            OrganizationId = request.OrganizationId.Value,
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            DateOfBirth = request.DateOfBirth,
                            NationalId = request.NationalId,
                            PhoneNumber = request.PhoneNumber,

                            OrganizationRole = OrganizationRole.Rider,
                            CreatedById = request.CreatedById
                        };
                        _context.Users.Add(user);
                        break;
                }
                break;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"DB Update Error: {ex.InnerException?.Message}");
            throw;
        }

        return Ok(new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            GlobalRole = user.GlobalRole,
            IsActive = user.IsActive
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var tokenEntry = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.IsRevoked);

        if (tokenEntry == null || tokenEntry.ExpiresAt < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token");

        var user = await _context.Users.FindAsync(tokenEntry.UserId);
        if (user == null) return Unauthorized("User not found");

        var newAccessToken = GenerateJwtToken(user);

        return Ok(new
        {
            AccessToken = newAccessToken,
            RefreshToken = request.RefreshToken, // reuse same until expired/revoked
            ExpiresIn = 3 * 60 * 60
        });
    }



    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username &&
                                      u.PasswordHash == HashPassword(request.Password));
        if (user == null)
            return Unauthorized("Invalid username or password");
        // 🔑 Generate Access Token
        var accessToken = GenerateJwtToken(user);

        // 🔄 Generate Refresh Token
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        // Save refresh token in DB (with expiry)
        var tokenEntry = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        _context.RefreshTokens.Add(tokenEntry);
        await _context.SaveChangesAsync();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Rider/User ID
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("Role", user.GlobalRole.ToString())
        };


        // after validating user
        if (user.OrganizationRole == OrganizationRole.Rider)
        {
            var session = new RiderSession
            {
                RiderId = user.Id
            };
            _context.RiderSessions.Add(session);
            await _context.SaveChangesAsync();
        }


        return Ok(new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3 * 60 * 60, // 3 hours in seconds
            message = "Login successful",
            user.Id,
            user.Username,
            user.GlobalRole
        });



    }

    [HttpPut("approve/superadmin/{userId:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ApproveBySuperAdmin(Guid userId)
    {
        // 🔑 Verify JWT
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue("Role");

        // No identity (JWT missing/invalid) → 401
        if (callerId == null)
            return Unauthorized("Please login and try again!");

        // Identity exists but not enough privileges → 403
        if (role != GlobalRole.SuperAdmin.ToString())
            return StatusCode(StatusCodes.Status403Forbidden,
        new { message = "Only SuperAdmins can approve users." });


        var userToApprove = await _context.Users.FindAsync(userId);
        if (userToApprove == null)
            return NotFound("User not found.");

        if (userToApprove.IsSuperAdminApproved == true)
            return BadRequest("User is already approved by SuperAdmin.");

        userToApprove.IsSuperAdminApproved = true;
        userToApprove.ApprovedById = Guid.Parse(callerId);
        userToApprove.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        var message = $"User {userToApprove.Username} #{userId} is approved by superadmin";
        if (userToApprove.OrganizationId == null || userToApprove.OrganizationId == Guid.Empty) return BadRequest("User is not part of any organization!");
        await _notificationService.NotifyOrganizationOwnerAsync(userToApprove!.OrganizationId ?? Guid.NewGuid(), message);
        await _notificationService.NotifyOrganizationAdminAsync(userToApprove!.OrganizationId ?? Guid.NewGuid(), message);
        await _notificationService.NotifySuperAdminAsync(message);
        await _notificationService.NotifyUserAsync(userId, message);

        return Ok(new { message = "User approved by SuperAdmin.", userToApprove.Id, userToApprove.IsSuperAdminApproved });
    }


    [HttpPut("approve/orgowner/{userId:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ApproveByOrgOwner(Guid userId)
    {
        // 🔑 Verify JWT
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue("Role");

        if (callerId == null || role != GlobalRole.OrgUser.ToString())
            return Forbid("Only OrgUsers (OrgOwner specifically) can approve users.");

        var orgOwner = await _context.Users.FindAsync(Guid.Parse(callerId));
        if (orgOwner == null || orgOwner.OrganizationRole != OrganizationRole.Owner)
            return Forbid("Caller must be an Organization Owner.");

        var userToApprove = await _context.Users.FindAsync(userId);
        if (userToApprove == null)
            return NotFound("User not found.");

        if (userToApprove.OrganizationId != orgOwner.OrganizationId)
            return Forbid("Cannot approve users outside your organization.");

        if (userToApprove.IsOrgOwnerApproved)
            return BadRequest("User is already approved by OrgOwner.");

        userToApprove.IsOrgOwnerApproved = true;
        userToApprove.ApprovedById = orgOwner.Id;
        userToApprove.ApprovedAt = DateTime.UtcNow;
        var orgId = userToApprove.OrganizationId;
        if (orgId == null) return BadRequest("User doesnt belong to any organization!");

        await _context.SaveChangesAsync();
        var message = $"User {userToApprove.Username} #{userId} is approved by Organization Owner";
        await _notificationService.NotifyOrganizationOwnerAsync(orgId ?? Guid.Empty, message);
        await _notificationService.NotifyOrganizationAdminAsync(orgId ?? Guid.Empty, message);
        await _notificationService.NotifyUserAsync(userId, message);


        return Ok(new { message = "User approved by OrgOwner.", userToApprove.Id, userToApprove.IsOrgOwnerApproved });
    }





    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }


    private string GenerateJwtToken(BaseUser user)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("OrgId", user.OrganizationId?.ToString() ?? ""),
        new Claim(ClaimTypes.Role, user.OrganizationRole?.ToString() ?? ""),
        new Claim("Role", user.GlobalRole.ToString() ?? "")

    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
