using System.Security.Claims;
using Delivera.Data;
using Delivera.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Delivera.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AdminActionsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly DeliveraDbContext _context;

    private readonly INotificationService _notificationService;


    public AdminActionsController(DeliveraDbContext context, IConfiguration config, INotificationService notificationService)
    {
        _context = context;
        _config = config;
        _notificationService = notificationService;
    }

    [HttpGet("users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetUsers()
    {
        var role = User.FindFirstValue("Role");

        if (role != GlobalRole.SuperAdmin.ToString())
        {
            return Unauthorized("Admin restricted!");
        }

        var users = await _context.Users.ToListAsync();
        if (users.IsNullOrEmpty())
        {
            return NotFound("No users were found!");
        }
        return Ok(users);

    }


    [HttpGet("organizations")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public async Task<IActionResult> GetOrganizations()
    {
        var role = User.FindFirstValue("Role");

        if (role != GlobalRole.SuperAdmin.ToString())
        {
            return Unauthorized("Admin restricted!");
        }


        var organizations = await _context.Organizations.ToListAsync();
        if (organizations.IsNullOrEmpty())
        {
            return NotFound("No organizations were found!");
        }
        return Ok(organizations);
    }

    [HttpPatch("superadmin/approveOrg/{organizationId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ApproveOrganization(Guid organizationId)
    {
        if (organizationId == null || organizationId == Guid.Empty)
        {
            return BadRequest("Organization Id is required!");
        }
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
    [HttpPatch("superadmin/revokeOrg/{organizationId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> RevokeOrganizationApproval(Guid organizationId)
    {
        if (organizationId == null || organizationId == Guid.Empty)
        {
            return BadRequest("Organization Id is required!");
        }
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
            return Unauthorized("Only administrator can revoke an organization!");
        }

        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId);
        if (org == null)
        {
            return NotFound("Organization not found!");
        }

        org.IsApproved = false;

        await _context.SaveChangesAsync();

        var message = $"Organization #{org.Id} {org.Name} approval is now revoked!";
        await _notificationService.NotifyOrganizationOwnerAsync(org.Id, message);
        await _notificationService.NotifySuperAdminAsync(message);


        return Ok("Organization approval is now revoked!");

    }

    [HttpPut("superadmin/approveuser/{userId:guid}")]
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



    [HttpPut("orgowner/approveuser/{userId:guid}")]
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



}