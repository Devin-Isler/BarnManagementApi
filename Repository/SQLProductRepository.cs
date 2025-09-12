using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Repository
{
    public class SQLProductRepository : IProductRepository
    {
        private readonly BarnDbContext context;

        public SQLProductRepository(BarnDbContext context)
        {
            this.context = context;
        }

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
                .ThenInclude(a => a.Farm)
                .Where(p => p.Animal != null 
                            && p.Animal.Farm != null 
                            && p.Animal.Farm.UserId == userId)
                .AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(filterOn) && !string.IsNullOrEmpty(filterQuery))
            {
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    product = product.Where(x => x.Name.Contains(filterQuery));
                }
                else if (filterOn.Equals("FarmId", StringComparison.OrdinalIgnoreCase))
                {
                    if (Guid.TryParse(filterQuery, out var farmId))
                    {
                        product = product.Where(x => x.Animal.FarmId == farmId);
                    }
                }
                else if (filterOn.Equals("AnimalId", StringComparison.OrdinalIgnoreCase))
                {
                    if (Guid.TryParse(filterQuery, out var animalId))
                    {
                        product = product.Where(x => x.AnimalId == animalId);
                    }
                }
                else if (filterOn.Equals("IsSold", StringComparison.OrdinalIgnoreCase) 
                    || filterOn.Equals("Sold", StringComparison.OrdinalIgnoreCase))
                {
                    if (bool.TryParse(filterQuery, out var isSold))
                    {
                        product = product.Where(x => x.IsSold == isSold);
                    }
                }
            }
            else
            {
                product = product.Where(x => x.IsSold == false);
            }
            // Sorting
            if(string.IsNullOrWhiteSpace(sortBy) == false)
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    product = isAscending ? product.OrderBy(x => x.Name): product.OrderByDescending(x => x.Name);
                }
                if(sortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                {
                    product = isAscending ? product.OrderBy(x => x.CreatedAt): product.OrderByDescending(x => x.CreatedAt);
                }
            }

            // Paging
            var skipResults = (pageNumber - 1) * pageSize;
            return await product
                .Skip(skipResults)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            var now = DateTime.UtcNow;
            return await context.Products
                .Include(p => p.Animal)
                .ThenInclude(a => a.Farm)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Product> ProduceProductAsync(Guid animalId)
        {
            // 1. Animal ve AnimalType'ı çek
            var animal = await context.Animals
                .Include(a => a.AnimalType)
                .FirstOrDefaultAsync(a => a.Id == animalId);

            if (animal == null) 
                throw new Exception("Animal not found");

            if (animal.AnimalType == null)
                throw new Exception("AnimalType not found for this animal");

            // 2. ProductType'ı bul (AnimalType.ProducedProductName ile eşleşiyor)
            var producedName = animal.AnimalType.ProducedProductName;
            var productType = await context.ProductTypes
                .FirstOrDefaultAsync(p => p.Name.ToLower() == producedName.ToLower());

            if (productType == null)
                throw new Exception("ProductType not found for this AnimalType");

            // 3. Product oluştur
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

        public async Task<Product?> UpdateProductAsync(Guid id, Product product)
        {
            var existing = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return null;
            }
            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.AnimalId = product.AnimalId;
            existing.SoldAt = product.SoldAt;
            existing.IsSold = existing.SoldAt != null;

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<Product?> DeleteProductAsync(Guid id)
        {
            var existing = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return null;
            }
            context.Products.Remove(existing);
            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<Product?> SellProductAsync(Guid id)
        {
            var existing = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return null;
            }
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
                        owner.Balance += existing.Price;
                    }
                }
            } 
            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<List<Product>> GetProductsByAnimalAsync(Guid animalId)
        {
            return await context.Products.Where(p => p.AnimalId == animalId).ToListAsync();
        }

        public async Task<List<Product>> GetAvailableProductsByFarmAsync(Guid farmId)
        {
            return await context.Products
                .Where(p => p.SoldAt == null)
                .Join(context.Animals, p => p.AnimalId, a => a.Id, (p, a) => new { p, a })
                .Where(x => x.a.FarmId == farmId)
                .Select(x => x.p)
                .ToListAsync();
        }
    }
}


