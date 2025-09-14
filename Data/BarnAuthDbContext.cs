// Barn Authentication Database Context - Handles user authentication and authorization
// Manages ASP.NET Identity tables and roles

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Data
{
    public class BarnAuthDbContext : IdentityDbContext
    {
        public BarnAuthDbContext(DbContextOptions<BarnAuthDbContext> options) : base(options)
        {
            
        }

        // Configure authentication entities and seed initial roles
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Define role IDs for consistency
            var readerRoleId = "d3f5a1c8-9b2e-4f7d-8c6a-2e1b3f9d7a4e";
            var writerRoleId = "a7c2e5f1-4d8b-42f0-9b3c-6e1d2a7f8b9c";

            // Seed initial roles
            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = readerRoleId,
                    ConcurrencyStamp= readerRoleId,
                    Name = "Reader",
                    NormalizedName = "Reader".ToUpper()
                },
                new IdentityRole
                {
                    Id = writerRoleId,
                    ConcurrencyStamp= writerRoleId,
                    Name = "Writer",
                    NormalizedName = "Writer".ToUpper()
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}