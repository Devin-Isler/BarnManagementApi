// Product Repository Implementation - Defines data access operations for products
// Provides abstraction layer for product-related database operations

using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Repository
{
    public class SQLProductRepository : IProductRepository
    {
        // Dependencies for animal operations 
        private readonly BarnDbContext context; // Access database

        public SQLProductRepository(BarnDbContext context)
        {
            this.context = context;
        }

        // Retrieves all products for a user, with optional filtering, sorting, and paging
        public async Task<List<Product>> GetAllProductsAsync(
            Guid userId, 
            string? filterOn = null, 
            string? filterQuery = null, 
            string? sortBy = null, 
            bool isAscending = false, 
            int pageNumber = 1, 
            int pageSize = 1000)
        {
            var product = context.Products
                .Include(p => p.Animal)
                .ThenInclude(a => a.Farm) // Include animal and its farm
                .Where(p => p.Animal != null 
                            && p.Animal.Farm != null 
                            && p.Animal.Farm.UserId == userId)
                .AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(filterOn) && !string.IsNullOrEmpty(filterQuery))
            {
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    product = product.Where(x => x.Name.Contains(filterQuery));
                }
                else if (filterOn.Equals("FarmId", StringComparison.OrdinalIgnoreCase) && Guid.TryParse(filterQuery, out var farmId))
                {
                    product = product.Where(x => x.Animal.FarmId == farmId);
                }
                else if (filterOn.Equals("AnimalId", StringComparison.OrdinalIgnoreCase) && Guid.TryParse(filterQuery, out var animalId))
                {
                    product = product.Where(x => x.AnimalId == animalId);
                }
                else if ((filterOn.Equals("IsSold", StringComparison.OrdinalIgnoreCase) || filterOn.Equals("Sold", StringComparison.OrdinalIgnoreCase))
                         && bool.TryParse(filterQuery, out var isSold))
                {
                    product = product.Where(x => x.IsSold == isSold);
                }
            }
            else
            {
                // Default: return only unsold products
                product = product.Where(x => x.IsSold == false);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    product = isAscending ? product.OrderBy(x => x.Name) : product.OrderByDescending(x => x.Name);
                }
                if (sortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                {
                    product = isAscending ? product.OrderBy(x => x.CreatedAt) : product.OrderByDescending(x => x.CreatedAt);
                }
            }

            // Pagination
            var skipResults = (pageNumber - 1) * pageSize;

            return await product
                .Skip(skipResults) // Skip previous pages
                .Take(pageSize)    // Take current page
                .ToListAsync();
        }

        // Retrieve a single unsold product by its ID, including animal and farm
        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await context.Products
                .Include(p => p.Animal)
                .ThenInclude(a => a.Farm)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsSold == false);
        }

        // Produce a new product for a given animal
        public async Task<Product> ProduceProductAsync(Guid animalId)
        {
            // Get animal and its type
            var animal = await context.Animals
                .Include(a => a.AnimalType)
                .FirstOrDefaultAsync(a => a.Id == animalId);
            if (animal == null) throw new Exception("Animal not found");
            if (animal.AnimalType == null) throw new Exception("AnimalType not found for this animal");

            // Find corresponding ProductType by name
            var producedName = animal.AnimalType.ProducedProductName;
            var productType = await context.ProductTypes.FirstOrDefaultAsync(p => p.Name.ToLower() == producedName.ToLower());
            if (productType == null) throw new Exception("ProductType not found for this AnimalType");

            // Create the product
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = productType.Name,
                Price = productType.DefaultSellPrice,
                AnimalId = animal.Id,
                CreatedAt = DateTime.UtcNow,
                SoldAt = null,
                IsSold = false,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();
            return product;
        }

        // Update product details
        public async Task<Product?> UpdateProductAsync(Guid id, Product product)
        {
            var existing = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return null;

            existing.Name = product.Name;
            existing.Price = product.Price;

            await context.SaveChangesAsync();
            return existing;
        }

        // Delete a product
        public async Task<Product?> DeleteProductAsync(Guid id)
        {
            var existing = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return null;

            context.Products.Remove(existing);
            await context.SaveChangesAsync();
            return existing;
        }

        // Sell a product and add its price to owner's balance
        public async Task<Product?> SellProductAsync(Guid id)
        {
            var existing = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return null;

            var animal = await context.Animals.FirstOrDefaultAsync(a => a.Id == existing.AnimalId);
            if (animal != null)
            {
                var farm = await context.Farms.FirstOrDefaultAsync(f => f.Id == animal.FarmId);
                if (farm != null)
                {
                    var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == farm.UserId);
                    if (owner != null)
                    {
                        existing.IsSold = true;
                        existing.SoldAt = DateTime.UtcNow; 
                        owner.Balance += existing.Price; // Add product price to owner balance
                    }
                }
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}