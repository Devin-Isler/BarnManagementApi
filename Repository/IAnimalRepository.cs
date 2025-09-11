using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Repository
{
    public interface IAnimalRepository
    {
        Task<List<Animal>> GetAllAnimalsAsync(Guid userId, string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = false, int pageNumber = 1, int pageSize=1000);
        Task<Animal?> GetAnimalByIdAsync(Guid id);
        Task<Animal?> BuyAnimalAsync(Animal animal);
        Task<Animal?> UpdateAnimalAsync(Guid id, Animal animal);
        Task<Animal?> SellAnimalAsync(Guid id);
        Task<Animal?> DeleteAnimalAsync(Guid id);
    }
}
 