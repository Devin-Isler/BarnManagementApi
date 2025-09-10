using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Repository
{
    public class SQLFarmRepository : IFarmRepository
    {
        private readonly BarnDbContext context;

        public SQLFarmRepository(BarnDbContext context)
        {
            this.context = context;
        }

        public async Task<List<Farm>> GetAllFarmsAsync()
        {
            return await context.Farms.ToListAsync();
        }
        public async Task<List<Farm>> GetFarmsByUserAsync(Guid userId)
        {
            return await context.Farms.Where(f => f.UserId == userId).ToListAsync();
        }
        public async Task<Farm> CreateFarmAsync(Farm farm)
        {
            await context.Farms.AddAsync(farm);
            await context.SaveChangesAsync();
            return farm;
        }

        public async Task<Farm?> UpdateFarmAsync(Guid id, Farm farm)
        {
            var existingRegion = await context.Farms.FirstOrDefaultAsync(x => x.Id == id);

            if (existingRegion == null)
            {
                return null;
            }

            existingRegion.LastUpdatedAt = DateTime.UtcNow;
            existingRegion.Name = farm.Name;
            existingRegion.Description = farm.Description;
            existingRegion.Location = farm.Location;

            await context.SaveChangesAsync();
            return existingRegion;
        }
        public async Task<Farm?> DeleteFarmAsync(Guid id)
        {
            var existingFarm = await context.Farms.FirstOrDefaultAsync(x => x.Id == id);

            if (existingFarm == null)
            {
                return null;
            }
            context.Farms.Remove(existingFarm);
            await context.SaveChangesAsync();
            return existingFarm;
        }

        public async Task<Farm?> GetFarmByIdAsync(Guid id)
        {
            return await context.Farms.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
