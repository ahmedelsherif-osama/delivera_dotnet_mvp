using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Data.Sqlite;
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
        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Delivera.Models.Location> Locations { get; set; } = null!;

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Zone> Zones { get; set; }


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
                IsOrgOwnerApproved = true,
                IsSuperAdminApproved = true,
                FirstName = "System",
                LastName = "Admin",
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<RefreshToken>()
           .HasOne(r => r.User)
           .WithMany() // keep simple, no back-collection needed for MVP
           .HasForeignKey(r => r.UserId)
           .OnDelete(DeleteBehavior.Cascade);

           modelBuilder.Entity<Zone>()
           .HasOne(z => z.Org)
           .WithMany()
           .HasForeignKey(z => z.OrganizationId)
           .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BaseUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

            modelBuilder.Entity<BaseUser>()
            .HasIndex(u => u.Username)
            .IsUnique();

            // Relationship: CreatedBy
            modelBuilder.Entity<BaseUser>().HasOne(u => u.CreatedByUser)
                  .WithMany() // no back-collection defined
                  .HasForeignKey(u => u.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relationship: ApprovedBy
            modelBuilder.Entity<BaseUser>().HasOne(u => u.ApprovedByUser)
                  .WithMany()
                  .HasForeignKey(u => u.ApprovedById)
                  .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Organization>()
    .HasOne(o => o.Owner)
    .WithMany()  // or .WithMany(u => u.OrganizationsOwned)
    .HasForeignKey(o => o.OwnerId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>().OwnsOne(o => o.PickUpLocation);
            modelBuilder.Entity<Order>().OwnsOne(o => o.DropOffLocation);
             modelBuilder.Entity<Zone>()
    .Property(z => z.Area)
    .HasConversion(
        v => v.AsText(),                // Polygon -> WKT string
        v => new WKTReader().Read(v) as Polygon // WKT string -> Polygon
    );





        }
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

       


    }



}