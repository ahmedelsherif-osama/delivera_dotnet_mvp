using Microsoft.EntityFrameworkCore;
using Delivera.Data;
using System.Text.Json.Serialization;
using Delivera.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);






builder.Services.AddDbContext<DeliveraDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")).EnableDetailedErrors().EnableSensitiveDataLogging()
);



static string HashPassword(string password)
{
    using var sha = SHA256.Create();
    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(bytes);
}


builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "Delivera",
        ValidAudience = "DeliveraUsers",
        IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<INotificationService, NotificationService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeliveraDbContext>();
    db.Database.EnsureCreated();
}


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DeliveraDbContext>();

    var superAdminId = Guid.Parse("D87D1DB6-9321-4323-9AF1-6157F85D2744");

    if (!context.Users.Any(u => u.Id == superAdminId))
    {
        var passwordHash = HashPassword("ChangeMe123!");
        context.Users.Add(new BaseUser
        {
            Id = superAdminId,
            Email = "superadmin@delivera.com",
            Username = "superadmin",
            PasswordHash = passwordHash,
            GlobalRole = GlobalRole.SuperAdmin,
            IsOrgOwnerApproved = true,
            IsSuperAdminApproved = true,
            FirstName = "System",
            LastName = "Admin",
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }
}

app.Run();
