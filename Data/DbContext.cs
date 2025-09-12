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
        public DbSet<RiderSession> RiderSessions { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                  .OnDelete(DeleteBehavior.Cascade);

            // Relationship: ApprovedBy
            modelBuilder.Entity<BaseUser>().HasOne(u => u.ApprovedByUser)
                  .WithMany()
                  .HasForeignKey(u => u.ApprovedById)
                  .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Organization>()
    .HasOne(o => o.Owner)
    .WithMany()  // or .WithMany(u => u.OrganizationsOwned)
    .HasForeignKey(o => o.OwnerId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>().OwnsOne(o => o.PickUpLocation);
            modelBuilder.Entity<Order>().OwnsOne(o => o.DropOffLocation);
            modelBuilder.Entity<Zone>()
   .Property(z => z.Area)
   .HasConversion(
       v => v.AsText(),                // Polygon -> WKT string
       v => new WKTReader().Read(v) as Polygon // WKT string -> Polygon
   );

            modelBuilder.Entity<RiderSession>().HasOne(s => s.Zone).WithMany().HasForeignKey(s => s.ZoneId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Order>()
    .HasOne<RiderSession>()
    .WithMany(rs => rs.ActiveOrders)
    .HasForeignKey(o => o.RiderSessionId)
    .OnDelete(DeleteBehavior.SetNull);






        }




    }



}