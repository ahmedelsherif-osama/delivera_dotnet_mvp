using Microsoft.EntityFrameworkCore;
using Delivera.Data;
using System.Text.Json.Serialization;
using Delivera.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Delivera.Services;


var builder = WebApplication.CreateBuilder(args);


// in Program.cs
builder.Services.AddSingleton<SessionManager>();



builder.Services.AddDbContext<DeliveraDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")).EnableDetailedErrors().EnableSensitiveDataLogging()
);





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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeliveraDbContext>();
    db.Database.EnsureCreated();
}


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
