using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Repository
{
    public class SQLAnimalRepository : IAnimalRepository
    {
        private readonly BarnDbContext context;

        public SQLAnimalRepository(BarnDbContext context)
        {
            this.context = context;
        }

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

            // Filtering
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
                // Default: sadece aktif hayvanlar
                animal = animal.Where(x => x.IsActive);
            }
            // Sorting
            if(string.IsNullOrWhiteSpace(sortBy) == false)
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    animal = isAscending ? animal.OrderBy(x => x.Name): animal.OrderByDescending(x => x.Name);
                }
                if(sortBy.Equals("LastProduction", StringComparison.OrdinalIgnoreCase))
                {
                    animal = isAscending ? animal.OrderBy(x => x.LastProductionTime): animal.OrderByDescending(x => x.LastProductionTime);
                }
                if(sortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                {
                    animal = isAscending ? animal.OrderBy(x => x.CreatedAt): animal.OrderByDescending(x => x.CreatedAt);
                }
            }

            // Paging
            var skipResults = (pageNumber - 1) * pageSize;
            return await animal
                .Include(f => f.Products)
                .Skip(skipResults)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Animal?> GetAnimalByIdAsync(Guid id)
        {
            var now = DateTime.UtcNow;
            return await context.Animals
                .Include(a => a.Farm)
                .Include(f => f.Products)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Animal?> BuyAnimalAsync(Animal animal)
        {
            await context.Animals.AddAsync(animal);
            animal.DeathTime = animal.CreatedAt.AddMinutes(animal.Lifetime);
            // Adjust owner's balance based on the animal's farm owner
            var farm = await context.Farms.FirstOrDefaultAsync(f => f.Id == animal.FarmId);
            if (farm != null)
            {
                var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == farm.UserId);
                if (owner != null)
                {
                    if(owner.Balance < animal.PurchasePrice)
                    {
                        return null;
                    }
                    else
                    {
                        owner.Balance -= animal.PurchasePrice;
                    }
                }
            }
            await context.SaveChangesAsync();
            return animal;
        }

        public async Task<Animal?> BuyAnimalByTemplateNameAsync(string templateName, Guid farmId)
        {
            var template = await context.AnimalType.FirstOrDefaultAsync(t => t.Name == templateName);
            if (template == null)
            {
                return null;
            }

            var farm = await context.Farms.FirstOrDefaultAsync(f => f.Id == farmId);
            if (farm == null)
            {
                return null;
            }

            var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == farm.UserId);
            if (owner == null)
            {
                return null;
            }

            if (owner.Balance < template.PurchasePrice)
            {
                return null;
            }

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

        public async Task<Animal?> UpdateAnimalAsync(Guid id, Animal animal)
        {
            var existing = await context.Animals.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return null;
            }
            var farm = await context.Farms.FirstOrDefaultAsync(f => f.Id == existing.FarmId);
            if (farm != null)
            {
                var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == farm.UserId);
                if (owner != null)
                {
                    owner.Balance += existing.PurchasePrice;
                    owner.Balance -= animal.PurchasePrice;
                }
            }
            existing.Name = animal.Name;
            existing.Lifetime = animal.Lifetime;
            existing.ProductionInterval = animal.ProductionInterval;
            existing.PurchasePrice = animal.PurchasePrice;
            existing.SellPrice = animal.SellPrice;
            existing.FarmId = animal.FarmId;
            existing.LastProductionTime = animal.LastProductionTime;
            // Recalculate death time from CreatedAt and updated Lifetime
            existing.DeathTime = existing.CreatedAt.AddMinutes(existing.Lifetime);
            
            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<Animal?> SellAnimalAsync(Guid id)
        {
            
            var existing = await context.Animals.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return null;
            }
            var farm = await context.Farms.FirstOrDefaultAsync(f => f.Id == existing.FarmId);
            if (farm != null)
            {
                var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == farm.UserId);
                if (owner != null)
                {
                    existing.SoldAt = DateTime.UtcNow; 
                    owner.Balance += existing.SellPrice;
                    existing.IsActive = false;
                }
            }
            await context.SaveChangesAsync();
            return existing;
        }
        public async Task<Animal?> DeleteAnimalAsync(Guid id)
        {
            var existing = await context.Animals.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return null;
            }

            context.Animals.Remove(existing);
            await context.SaveChangesAsync();
            return existing;
        }
    }
}
