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
        public DbSet<AnimalType> AnimalType { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal precisions
            modelBuilder.Entity<Animal>()
                .Property(a => a.PurchasePrice).HasPrecision(18, 2);
            modelBuilder.Entity<Animal>()
                .Property(a => a.SellPrice).HasPrecision(18, 2);

            modelBuilder.Entity<AnimalType>()
                .Property(t => t.PurchasePrice).HasPrecision(18, 2);
            modelBuilder.Entity<AnimalType>()
                .Property(t => t.DefaultSellPrice).HasPrecision(18, 2);
            modelBuilder.Entity<AnimalType>()
                .Property(t => t.ProducedProductSellPrice).HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price).HasPrecision(18, 2);

            modelBuilder.Entity<ProductType>()
                .Property(pt => pt.DefaultSellPrice).HasPrecision(18, 2);

            modelBuilder.Entity<User>()
                .Property(u => u.Balance).HasPrecision(18, 2);

            // Unique Name for AnimalTemplate
            modelBuilder.Entity<AnimalType>()
                .HasIndex(t => t.Name)
                .IsUnique();

            // Relationship Animal -> AnimalTemplate
            modelBuilder.Entity<Animal>()
                .HasOne(a => a.AnimalType)
                .WithMany(t => t.Animals)
                .HasForeignKey(a => a.AnimalTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed ProductTypes with deterministic IDs
            var eggTypeId = new Guid("66666666-6666-6666-6666-666666666666");
            var milkTypeId = new Guid("77777777-7777-7777-7777-777777777777");
            var woolTypeId = new Guid("88888888-8888-8888-8888-888888888888");
            var goatMilkTypeId = new Guid("99999999-9999-9999-9999-999999999999");
            var duckEggTypeId = new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

            modelBuilder.Entity<ProductType>().HasData(
                new ProductType { Id = eggTypeId, Name = "Egg", DefaultSellPrice = 1m },
                new ProductType { Id = milkTypeId, Name = "Milk", DefaultSellPrice = 3m },
                new ProductType { Id = woolTypeId, Name = "Wool", DefaultSellPrice = 4m },
                new ProductType { Id = goatMilkTypeId, Name = "Goat Milk", DefaultSellPrice = 2m },
                new ProductType { Id = duckEggTypeId, Name = "Duck Egg", DefaultSellPrice = 2m }
            );
        }
    }
}