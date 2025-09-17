// Animal Repository Implementation - Defines data access operations for animals
// Provides abstraction layer for animal-related database operations

using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Repository
{
    public class SQLAnimalRepository : IAnimalRepository
    {
        // Dependencies for animal operations
        private readonly BarnDbContext context; // Access database 

        public SQLAnimalRepository(BarnDbContext context)
        {
            this.context = context;
        }

        // Retrieves all animals for a user, with optional filtering, sorting, and paging
        public async Task<List<Animal>> GetAllAnimalsAsync(
            Guid userId, 
            string? filterOn = null, 
            string? filterQuery = null, 
            string? sortBy = null, 
            bool isAscending = false, 
            int pageNumber = 1, 
            int pageSize = 1000)
        {   
            var animal = context.Animals
                .Include(a => a.Farm)
                .Where(a => a.Farm != null && a.Farm.UserId == userId)
                .AsQueryable();

            // Apply filtering based on the provided filter field and query
            if (!string.IsNullOrEmpty(filterOn) && !string.IsNullOrEmpty(filterQuery))
            {
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    animal = animal.Where(x => x.Name.Contains(filterQuery));
                }
                else if (filterOn.Equals("FarmId", StringComparison.OrdinalIgnoreCase))
                {
                    if (Guid.TryParse(filterQuery, out var farmId))
                    {
                        animal = animal.Where(x => x.FarmId == farmId);
                    }
                }
                else if (filterOn.Equals("IsActive", StringComparison.OrdinalIgnoreCase) 
                    || filterOn.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    if (bool.TryParse(filterQuery, out var isActive))
                    {
                        animal = animal.Where(x => x.IsActive == isActive);
                    }
                }
            }
            else
            {
                // Default: return only active animals
                animal = animal.Where(x => x.IsActive);
            }

            // Apply sorting based on the specified field and direction
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    animal = isAscending ? animal.OrderBy(x => x.Name) : animal.OrderByDescending(x => x.Name);
                }
                if (sortBy.Equals("LastProduction", StringComparison.OrdinalIgnoreCase))
                {
                    animal = isAscending ? animal.OrderBy(x => x.LastProductionTime) : animal.OrderByDescending(x => x.LastProductionTime);
                }
                if (sortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                {
                    animal = isAscending ? animal.OrderBy(x => x.CreatedAt) : animal.OrderByDescending(x => x.CreatedAt);
                }
            }

            // Apply pagination
            var skipResults = (pageNumber - 1) * pageSize;
            return await animal
                .Include(f => f.Products) // Include related products
                .Skip(skipResults)
                .Take(pageSize)
                .ToListAsync();
        }

        // Retrieve a single active animal by its ID, including its farm and products
        public async Task<Animal?> GetAnimalByIdAsync(Guid id)
        {
            return await context.Animals
                .Include(a => a.Farm)
                .Include(f => f.Products)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        }

        // Buy animal using a predefined template
        public async Task<Animal?> BuyAnimalByTemplateNameAsync(string templateName, Guid farmId)
        {
            var template = await context.AnimalType.FirstOrDefaultAsync(t => t.Name == templateName); // Find the predefined template
            if (template == null) return null;

            var farm = await context.Farms.FirstOrDefaultAsync(f => f.Id == farmId);
            if (farm == null) return null;

            var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == farm.UserId);
            if (owner == null || owner.Balance < template.PurchasePrice) return null;

            // Create animal by default values from the template
            var animal = new Animal
            {
                Id = Guid.NewGuid(),
                Name = template.Name,
                Lifetime = template.Lifetime,
                ProductionInterval = template.ProductionInterval,
                PurchasePrice = template.PurchasePrice,
                SellPrice = template.DefaultSellPrice,
                FarmId = farmId,
                AnimalTypeId = template.Id,
                CreatedAt = DateTime.UtcNow,
                LastProductionTime = null,
                IsActive = true
            };
            animal.DeathTime = animal.CreatedAt.AddMinutes(animal.Lifetime);

            owner.Balance -= template.PurchasePrice;
            await context.Animals.AddAsync(animal);
            await context.SaveChangesAsync();
            return animal;
        }

        // Update animal details and adjust owner's balance accordingly
        public async Task<Animal?> UpdateAnimalAsync(Guid id, Animal animal)
        {
            var existing = await context.Animals.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return null;

            var farm = await context.Farms.FirstOrDefaultAsync(f => f.Id == existing.FarmId);
            if (farm != null)
            {
                var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == farm.UserId);
                if (owner != null)
                {
                    existing.Name = animal.Name;
                }
            }
            await context.SaveChangesAsync();
            return existing;
        }

        // Sell an animal and add sell price to owner's balance
        public async Task<Animal?> SellAnimalAsync(Guid id)
        {
            var existing = await context.Animals.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return null;

            var farm = await context.Farms.FirstOrDefaultAsync(f => f.Id == existing.FarmId); // Check the farm
            if (farm != null)
            {
                var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == farm.UserId); // Check the user
                if (owner != null)
                {
                    // Adjust necessary variables
                    existing.SoldAt = DateTime.UtcNow;
                    owner.Balance += existing.SellPrice;
                    existing.IsActive = false;
                }
            }

            await context.SaveChangesAsync();
            return existing;
        }

        // Delete an animal along with its products
        public async Task<(Animal? animal, int productsCount)> DeleteAnimalAsync(Guid id)
        {
            var existing = await context.Animals.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return (null, 0);

            // Check for the products that the animal has
            var products = await context.Products.Where(p => p.AnimalId == existing.Id).ToListAsync();
            var productsCount = products.Count;
            if (products.Count > 0)
            {
                context.Products.RemoveRange(products);
            }

            context.Animals.Remove(existing);
            await context.SaveChangesAsync();
            return (existing, productsCount);
        }
    }
}