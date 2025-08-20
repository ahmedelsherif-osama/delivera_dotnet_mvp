using Microsoft.AspNetCore.Mvc;
using Delivera.Models;
using Delivera.DTOs;
using Delivera.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Delivera.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly DeliveraDbContext _context;
    public AuthController(DeliveraDbContext context) => _context = context;

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email already registered.");

        if (request.GlobalRole == GlobalRole.SuperAdmin)
        {
            bool superAdminExists = await _context.Users
                .AnyAsync(u => u.GlobalRole == GlobalRole.SuperAdmin);

            if (superAdminExists)
                return BadRequest("A SuperAdmin already exists.");
        }


        var user = new BaseUser
        {
            Email = request.Email,
            Username = request.Username,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = HashPassword(request.Password),
            GlobalRole = request.GlobalRole,
            IsActive = request.GlobalRole == GlobalRole.SuperAdmin
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            GlobalRole = user.GlobalRole,
            IsActive = user.IsActive
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

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("Role", user.GlobalRole.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        return Ok(new { message = "Login successful", user.Id, user.Username, user.GlobalRole });
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
