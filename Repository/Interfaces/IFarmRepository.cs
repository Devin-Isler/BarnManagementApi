// Farm Repository Interface - Defines data access operations for farms
// Provides abstraction layer for farm-related database operations

using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Repository
{
    public interface IFarmRepository
    {
        // Get a specific farm by ID
        Task<Farm?> GetFarmByIdAsync(Guid id);
        
        // Create a new farm
        Task<Farm> CreateFarmAsync(Farm farm);
        
        // Update an existing farm
        Task<Farm?> UpdateFarmAsync(Guid id, Farm farm);
        
        // Hard delete a farm and return counts of deleted animals and products
        Task<(Farm? farm, int animalsCount, int productsCount)> DeleteFarmAsync(Guid id);
        
        // Get all farms for a user with filtering, sorting and pagination
        Task<List<Farm>> GetFarmsByUserAsync(Guid userId, string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = false, int pageNumber = 1, int pageSize=1000);
    }
}