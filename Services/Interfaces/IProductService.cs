using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Services.Interfaces
{
    public interface IProductService
    {
        Task<Product?> GetProductByIdAsync(Guid productId);
        Task<List<Product>> GetProductsByAnimalAsync(Guid animalId);
        Task<Product?> SellProductAsync(Guid userId, Guid productId);
        Task<List<Product>> GetAvailableProductsAsync(Guid farmId);
    }
}


