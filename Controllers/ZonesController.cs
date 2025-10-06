using System.Security.Claims;
using Delivera.Data;
using Delivera.DTOs;
using Delivera.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Delivera.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ZonesController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly DeliveraDbContext _context;

    private readonly INotificationService _notificationService;


    public ZonesController(DeliveraDbContext context, IConfiguration config, INotificationService notificationService)
    {
        _context = context;
        _config = config;
        _notificationService = notificationService;
    }


    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetZone(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var orgId = User.FindFirstValue("OrgId");


        if (string.IsNullOrEmpty(userId)) return Unauthorized("Invalid token");

        if (role == OrganizationRole.Rider.ToString() || role == OrganizationRole.Support.ToString())
            return Unauthorized("This user type cannot access zones");


        var zone = await _context.Zones.FindAsync(id);
        if (zone == null) return NotFound();
        var response = new ZoneResponse
        {
            Id = zone.Id,
            Name = zone.Name,
            WktPolygon = zone.Area.AsText()

        };

        return Ok(response);
    }
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetZones()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var orgId = User.FindFirstValue("OrgId");


        if (string.IsNullOrEmpty(userId)) return Unauthorized("Invalid token");

        if (role == OrganizationRole.Rider.ToString() || role == OrganizationRole.Support.ToString())
            return Unauthorized("This user type cannot access zones");


        var zones = await _context.Zones.Where(zone => zone.OrganizationId == Guid.Parse(orgId!)).Select(zone => new
       ZoneResponse
        {
            Id = zone.Id,
            Name = zone.Name,
            WktPolygon = zone.Area.AsText()

        }).ToListAsync();


        return Ok(zones);
    }

    [Authorize]
    [HttpDelete("delete/{zoneId}")]
    public async Task<IActionResult> DeleteZone(Guid zoneId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var orgId = User.FindFirstValue("OrgId");

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Invalid token");

        if (role == OrganizationRole.Rider.ToString() || role == OrganizationRole.Support.ToString())
            return Unauthorized("This user type cannot access zones");

        if (string.IsNullOrEmpty(orgId))
            return BadRequest("OrganizationId missing");

        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == Guid.Parse(orgId));

        if (org == null)
            return BadRequest("You don't belong to any organization!");

        var zone = await _context.Zones
            .FirstOrDefaultAsync(z => z.OrganizationId == Guid.Parse(orgId) && z.Id == zoneId);

        if (zone == null)
            return NotFound("Zone not found");

        _context.Zones.Remove(zone);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Zone deleted successfully" });
    }





    [Authorize]
    [HttpPost("create")]
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







}