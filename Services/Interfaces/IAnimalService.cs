using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Services.Interfaces
{
    public interface IAnimalService
    {
        Task<Animal?> BuyAnimalAsync(Guid userId, Guid farmId, Animal animal);
        Task<Animal?> SellAnimalAsync(Guid userId, Guid animalId);
        Task<Product?> ProduceProductAsync(Guid animalId);
        Task<Animal?> CheckAnimalLifetimeAsync(Guid animalId);
    }
}


