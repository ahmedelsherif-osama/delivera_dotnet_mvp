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
using Delivera.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Delivera.Controllers;


[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly DeliveraDbContext _context;

    private readonly SessionManager _sessionManager;

    public AuthController(DeliveraDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
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

        return CreatedAtAction(nameof(GetZone), new { id = zone.Id }, response);
    }

    [Authorize]
[HttpPost("assignRider")]
public ActionResult AssignRider(Guid orderId)
{
    // ✅ 1. Find order
    var order = _context.Orders.Find(orderId);
    if (order == null) return NotFound(new { error = "Order not found" });

    // ✅ 2. Check user permissions
    var role = User.FindFirstValue("role");
    if (role != OrganizationRole.Owner.ToString() &&
        role != OrganizationRole.Admin.ToString())
    {
        return Forbid("Only admins or owners can assign riders");
    }

    // ✅ 3. Determine zone (MVP = naive approach)
    var pickupPoint = new NetTopologySuite.Geometries.Point(
        order.PickUpLocation.Latitude,
        order.PickUpLocation.Longitude
    );

    var zone = _context.Zones.FirstOrDefault(z => z.Area.Contains(pickupPoint));
    if (zone == null)
        return NotFound(new { error = "Pickup location not in any zone" });

    // ✅ 4. Find nearest rider in that zone
    var rider = _sessionManager.GetNearestActiveRider(
        order.PickUpLocation.Latitude,
        order.PickUpLocation.Longitude,
        zone.Name
    );

    if (rider == null)
        return NotFound(new { error = "No active rider available in this zone" });

    // ✅ 5. Update order + rider
    order.Status = OrderStatus.Assigned;
    order.RiderId = rider.RiderId; // 🔴 for MVP only, better to store RiderId as Guid
    rider.ActiveOrders.Add(order.Id);

    _context.SaveChanges();

    return Ok(new
    {
        message = "Rider assigned",
        riderId = rider.RiderId
    });
}


    [Authorize]
    [HttpPost("createOrder")]
    public async Task<ActionResult> CreateOrder([FromBody] OrderRequest orderRequest)
    {
        if (orderRequest.OrderDetails == null || orderRequest.OrderDetails=="")
        {
            return BadRequest("Order details are required!");
        }
        if (orderRequest.OrganizationId == null )
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
            OrderDetails = orderRequest.OrderDetails
        };
        _context.Orders.Add(order);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"DB Update Error: {ex.InnerException?.Message}");
            throw;
        }

        return Ok(new { message = "Order created successfully!" });


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
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("Role", user.GlobalRole.ToString())
        };


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

    [HttpPost("approve/superadmin/{userId:guid}")]
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

        if (userToApprove.IsSuperAdminApproved)
            return BadRequest("User is already approved by SuperAdmin.");

        userToApprove.IsSuperAdminApproved = true;
        userToApprove.ApprovedById = Guid.Parse(callerId);
        userToApprove.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "User approved by SuperAdmin.", userToApprove.Id, userToApprove.IsSuperAdminApproved });
    }


    [HttpPost("approve/orgowner/{userId:guid}")]
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

        await _context.SaveChangesAsync();
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
        new Claim(ClaimTypes.Role, user.OrganizationRole?.ToString() ?? "")

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
