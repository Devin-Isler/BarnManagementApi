using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Repository
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> UpdateProductAsync(Guid id, Product product);
        Task<Product?> DeleteProductAsync(Guid id);
        Task<List<Product>> GetProductsByAnimalAsync(Guid animalId);
        Task<List<Product>> GetAvailableProductsByFarmAsync(Guid farmId);
    }
}


