using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
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

        public async Task<List<Animal>> GetAllAnimalsAsync()
        {
            return await context.Animals.ToListAsync();
        }

        public async Task<Animal?> GetAnimalByIdAsync(Guid id)
        {
            return await context.Animals.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Animal> CreateAnimalAsync(Animal animal)
        {
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
            existing.Name = animal.Name;
            existing.Lifetime = animal.Lifetime;
            existing.ProductionInterval = animal.ProductionInterval;
            existing.PurchasePrice = animal.PurchasePrice;
            existing.SellPrice = animal.SellPrice;
            existing.FarmId = animal.FarmId;
            existing.LastProductionTime = animal.LastProductionTime;
            existing.DeathTime = animal.DeathTime;

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


