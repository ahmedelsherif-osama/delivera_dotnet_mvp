using Delivera.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Delivera.Data
{
    public class DeliveraDbContext : DbContext
    {
        public DeliveraDbContext(DbContextOptions<DeliveraDbContext> options) : base(options) { }
        public DbSet<BaseUser> Users { get; set; }
    }
}