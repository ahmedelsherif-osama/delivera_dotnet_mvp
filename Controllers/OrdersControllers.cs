using System.Security.Claims;
using Delivera.Data;
using Delivera.DTOs;
using Delivera.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Delivera.Controllers;

[ApiController]
[Route("api/[controller]")]

public class OrdersController : ControllerBase
{
    // private readonly IConfiguration _config;
    private readonly DeliveraDbContext _context;

    private readonly INotificationService _notificationService;


    public OrdersController(DeliveraDbContext context, IConfiguration config, INotificationService notificationService)
    {
        _context = context;
        // _config = config;
        _notificationService = notificationService;
    }

    [HttpGet("orgorders")]
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

    [HttpGet("superadmin")]
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

    [HttpGet("own")]
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


    [HttpGet("{orderId}")]
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



}