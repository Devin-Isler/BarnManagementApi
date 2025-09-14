// Animal Repository Interface - Defines data access operations for animals
// Provides abstraction layer for animal-related database operations

using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;

namespace BarnManagementApi.Repository
{
    public interface IAnimalRepository
    {
        // Get all animals for a user with filtering, sorting and pagination
        Task<List<Animal>> GetAllAnimalsAsync(Guid userId, string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = false, int pageNumber = 1, int pageSize=1000);
        
        // Get a specific animal by ID
        Task<Animal?> GetAnimalByIdAsync(Guid id);
        
        // Buy an animal (create new animal instance)
        Task<Animal?> BuyAnimalAsync(Animal animal);
        
        // Buy an animal using template name and farm ID
        Task<Animal?> BuyAnimalByTemplateNameAsync(string templateName, Guid farmId);
        
        // Update an existing animal
        Task<Animal?> UpdateAnimalAsync(Guid id, Animal animal);
        
        // Sell an animal, soft delete, update user balance
        Task<Animal?> SellAnimalAsync(Guid id);
        
        // Hard delete an animal and return count of deleted products
        Task<(Animal? animal, int productsCount)> DeleteAnimalAsync(Guid id);
    }
}
 