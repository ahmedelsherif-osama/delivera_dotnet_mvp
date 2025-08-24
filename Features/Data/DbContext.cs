using Delivera.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
namespace Delivera.Data
{
    public class DeliveraDbContext : DbContext
    {
        public DeliveraDbContext(DbContextOptions<DeliveraDbContext> options) : base(options) { }
        public DbSet<BaseUser> Users { get; set; } = null!;
        public DbSet<Organization> Organizations { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Seed the initial SuperAdmin
            var superAdminId = Guid.NewGuid();
            var passwordHash = HashPassword("ChangeMe123!"); // load from config later

            modelBuilder.Entity<BaseUser>().HasData(new BaseUser
            {
                Id = superAdminId,
                Email = "superadmin@delivera.com",
                Username = "superadmin",
                PasswordHash = passwordHash,
                GlobalRole = GlobalRole.SuperAdmin,
                IsActive = true,
                FirstName = "System",
                LastName = "Admin",
                CreatedAt = DateTime.UtcNow
            });


        }
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}