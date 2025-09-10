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

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await context.Products.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            await context.Products.AddAsync(product);
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


