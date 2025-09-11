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

        public async Task<List<Farm>> GetFarmsByUserAsync(Guid userId, string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = false, int pageNumber = 1, int pageSize=1000)
        {
            var farm = context.Farms
                .Include(f => f.Animals.Where(a => a.IsActive))
                .Where(f => f.UserId == userId)
                .AsQueryable();

            // Filtering
            if(string.IsNullOrEmpty(filterOn) == false && string.IsNullOrEmpty(filterQuery) == false )
            {
                if(filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    farm = farm.Where(x => x.Name.Contains(filterQuery));
                }
                if(filterOn.Equals("Location", StringComparison.OrdinalIgnoreCase))
                {
                    farm = farm.Where(x => x.Location.Contains(filterQuery));
                }
            }

            // Sorting
            if(string.IsNullOrWhiteSpace(sortBy) == false)
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    farm = isAscending ? farm.OrderBy(x => x.Name): farm.OrderByDescending(x => x.Name);
                }
                if(sortBy.Equals("Location", StringComparison.OrdinalIgnoreCase))
                {
                    farm = isAscending ? farm.OrderBy(x => x.Location): farm.OrderByDescending(x => x.Location);
                }
                if(sortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                {
                    farm = isAscending ? farm.OrderBy(x => x.CreatedAt): farm.OrderByDescending(x => x.CreatedAt);
                }
            }

            // Pagination
            var skipResults = (pageNumber - 1) * pageSize;

            // return await regionDbContext.farm.Include("Difficulty").Include("Region").ToListAsync();
            return await farm.Skip(skipResults).Take(pageSize).ToListAsync();
        }

        public async Task<Farm> CreateFarmAsync(Farm farm)
        {
            await context.Farms.AddAsync(farm);
            await context.SaveChangesAsync();
            return farm;
        }

        public async Task<Farm?> UpdateFarmAsync(Guid id, Farm farm)
        {
            var existingFarm = await context.Farms.FirstOrDefaultAsync(x => x.Id == id);

            if (existingFarm == null)
            {
                return null;
            }

            existingFarm.LastUpdatedAt = DateTime.UtcNow;
            existingFarm.Name = farm.Name;
            existingFarm.Description = farm.Description;
            existingFarm.Location = farm.Location;

            await context.SaveChangesAsync();
            return existingFarm;
        }

        public async Task<Farm?> DeleteFarmAsync(Guid id)
        {
            var existingFarm = await context.Farms.FirstOrDefaultAsync(x => x.Id == id);

            if (existingFarm == null)
            {
                return null;
            }
            var animals = await context.Animals.Where(a => a.FarmId == existingFarm.Id).ToListAsync();
                if (animals.Count > 0)
                {
                    context.Animals.RemoveRange(animals);
                }
            context.Farms.Remove(existingFarm);
            await context.SaveChangesAsync();
            return existingFarm;
        }

        public async Task<Farm?> GetFarmByIdAsync(Guid id)
        {
            return await context.Farms
                .Include(f => f.Animals)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

    }
}
