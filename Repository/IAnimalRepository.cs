using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Repository
{
    public interface IAnimalRepository
    {
        Task<List<Animal>> GetAllAnimalsAsync();
        Task<Animal?> GetAnimalByIdAsync(Guid id);
        Task<Animal> CreateAnimalAsync(Animal animal);
        Task<Animal?> UpdateAnimalAsync(Guid id, Animal animal);
        Task<Animal?> DeleteAnimalAsync(Guid id);
    }
}


