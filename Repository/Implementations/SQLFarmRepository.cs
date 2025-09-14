// Farm Repository Implementation - Defines data access operations for farms
// Provides abstraction layer for farm-related database operations

using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Repository
{
    public class SQLFarmRepository : IFarmRepository
    {

        // Dependencies for animal operations        
        private readonly BarnDbContext context; // Access database 

        public SQLFarmRepository(BarnDbContext context)
        {
            this.context = context; 
        }

        // Retrieves all farms for a user, with optional filtering, sorting, and pagination
        public async Task<List<Farm>> GetFarmsByUserAsync(
            Guid userId, 
            string? filterOn = null, 
            string? filterQuery = null, 
            string? sortBy = null, 
            bool isAscending = false, 
            int pageNumber = 1, 
            int pageSize=1000)
        {
            var farm = context.Farms
                .Include(f => f.Animals.Where(a => a.IsActive)) // Only include active animals
                .Where(f => f.UserId == userId)
                .AsQueryable();

            // Apply filtering based on Name or Location
            if(!string.IsNullOrEmpty(filterOn) && !string.IsNullOrEmpty(filterQuery))
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

            // Apply sorting based on specified field
            if(!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    farm = isAscending ? farm.OrderBy(x => x.Name) : farm.OrderByDescending(x => x.Name);
                }
                if(sortBy.Equals("Location", StringComparison.OrdinalIgnoreCase))
                {
                    farm = isAscending ? farm.OrderBy(x => x.Location) : farm.OrderByDescending(x => x.Location);
                }
                if(sortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                {
                    farm = isAscending ? farm.OrderBy(x => x.CreatedAt) : farm.OrderByDescending(x => x.CreatedAt);
                }
            }

            // Apply pagination
            var skipResults = (pageNumber - 1) * pageSize;

            return await farm
                .Skip(skipResults) // Skip previous pages
                .Take(pageSize)    // Take current page
                .ToListAsync();
        }

        // Creates a new farm and saves it to the database
        public async Task<Farm> CreateFarmAsync(Farm farm)
        {
            await context.Farms.AddAsync(farm);
            await context.SaveChangesAsync();
            return farm;
        }

        // Updates an existing farm's details
        public async Task<Farm?> UpdateFarmAsync(Guid id, Farm farm)
        {
            var existingFarm = await context.Farms.FirstOrDefaultAsync(x => x.Id == id);
            if (existingFarm == null) return null;

            existingFarm.LastUpdatedAt = DateTime.UtcNow; // Update timestamp
            existingFarm.Name = farm.Name;
            existingFarm.Description = farm.Description;
            existingFarm.Location = farm.Location;

            await context.SaveChangesAsync();
            return existingFarm;
        }

        // Deletes a farm along with its animals and their products
        public async Task<(Farm? farm, int animalsCount, int productsCount)> DeleteFarmAsync(Guid id)
        {
            var existingFarm = await context.Farms.FirstOrDefaultAsync(x => x.Id == id);
            if (existingFarm == null) return (null, 0, 0);

            // Get all animals for this farm
            var animals = await context.Animals.Where(a => a.FarmId == existingFarm.Id).ToListAsync();
            var animalIds = animals.Select(a => a.Id).ToList();
            var animalsCount = animals.Count;

            var productsCount = 0;
            if (animalIds.Count > 0)
            {
                // Delete all products associated with these animals
                var products = await context.Products.Where(p => animalIds.Contains(p.AnimalId)).ToListAsync();
                productsCount = products.Count;
                if (products.Count > 0)
                {
                    context.Products.RemoveRange(products);
                }
                
                // Delete all animals
                context.Animals.RemoveRange(animals);
            }

            // Delete the farm itself
            context.Farms.Remove(existingFarm);
            await context.SaveChangesAsync();
            return (existingFarm, animalsCount, productsCount);
        }

        // Retrieve a single farm by its ID including its animals
        public async Task<Farm?> GetFarmByIdAsync(Guid id)
        {
            return await context.Farms
                .Include(f => f.Animals) // Include all animals for the farm
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}