using BarnManagementApi.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Data
{
    public class BarnDbContext : DbContext
    {
        public BarnDbContext(DbContextOptions<BarnDbContext> options) : base(options)
        {
        }

        public DbSet<Farm> Farms { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
    }
}