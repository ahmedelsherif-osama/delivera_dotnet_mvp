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
using Delivera.Helpers;

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
                            // OrganizationId = request.OrganizationId

                        };

                        _context.Users.Add(user);
                        await _context.SaveChangesAsync(); // ✅ ensure user.Id exists

                        var org = new Organization
                        {
                            IsApproved = false,
                            OwnerId = user.Id, // ✅ now safe
                        };
                        org.ShortCode = CodeGeneratorHelper.Base62Encode(org.Id);

                        _context.Organizations.Add(org);
                        await _context.SaveChangesAsync();

                        // optional: link user → org for convenience
                        user.OrganizationId = org.Id;
                        await _context.SaveChangesAsync();
                        break;

                    case OrganizationRole.Admin:
                        if (request.OrganizationShortCode.IsNullOrEmpty())
                            return BadRequest("Organization short code is required for Admin users.");

                        var org2 = await _context.Organizations.FirstOrDefaultAsync(o => o.ShortCode == request.OrganizationShortCode);
                        if (org2 == null) return NotFound("Invalid short code! Please check you organizations short code!");
                        var orgId2 = org2.Id;
                        user = new OrgAdmin
                        {
                            Email = request.Email,
                            Username = request.Username,
                            PasswordHash = HashPassword(request.Password),
                            GlobalRole = GlobalRole.OrgUser,
                            OrganizationId = orgId2,
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
                        if (request.OrganizationShortCode.IsNullOrEmpty())
                            return BadRequest("Organization short code is required for Support users.");

                        var org3 = await _context.Organizations.FirstOrDefaultAsync(o => o.ShortCode == request.OrganizationShortCode);
                        if (org3 == null) return NotFound("Invalid short code! Please check you organizations short code!");
                        var orgId3 = org3.Id;
                        user = new OrgSupport
                        {
                            Email = request.Email,
                            Username = request.Username,
                            PasswordHash = HashPassword(request.Password),
                            GlobalRole = GlobalRole.OrgUser,
                            OrganizationId = orgId3,
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
                        Console.WriteLine(request.OrganizationShortCode);
                        if (request.OrganizationShortCode.IsNullOrEmpty())
                            return BadRequest("Organization short code required for Rider users.");
                        var orgs = await _context.Organizations.ToListAsync();
                        var org4 = orgs.FirstOrDefault(o => o.ShortCode == request.OrganizationShortCode);
                        if (org4 == null) return NotFound("Invalid short code! Please check you organizations short code!");
                        var orgId4 = org4.Id;
                        user = new Rider
                        {
                            Email = request.Email,
                            Username = request.Username,
                            PasswordHash = HashPassword(request.Password),
                            GlobalRole = GlobalRole.OrgUser,
                            OrganizationId = orgId4,
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

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
            RefreshToken = request.RefreshToken,
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
            user.GlobalRole,
            user.OrganizationRole
        });



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

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        var tokenEntry = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.IsRevoked);

        if (tokenEntry == null)
        {

            return BadRequest("Invalid refresh token");
        }

        tokenEntry.IsRevoked = true;
        tokenEntry.RevokedAt = DateTime.UtcNow;

        Console.WriteLine($"vat is {userId}");


        if (role == OrganizationRole.Rider.ToString())
        {

            // var sessions = await _context.RiderSessions.ToListAsync();
            // Console.WriteLine(sessions);
            var session = await _context.RiderSessions.FirstOrDefaultAsync(s => s.RiderId.ToString().ToLower() == userId!.ToLower() && s.Status != SessionStatus.Completed);
            if (session != null)
            {
                Console.WriteLine("not null bro");
                session.Status = SessionStatus.Completed;
                await _context.SaveChangesAsync();


            }
            else
            {
                Console.WriteLine("session is null bro");
            }

        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Logged out successfully" });
    }


}
