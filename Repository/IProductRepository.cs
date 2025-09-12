using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Repository
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProductsAsync(Guid userId, string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = false, int pageNumber = 1, int pageSize=1000);
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<Product> ProduceProductAsync(Guid animalId);
        Task<Product?> UpdateProductAsync(Guid id, Product product);
        Task<Product?> DeleteProductAsync(Guid id);
        Task<Product?> SellProductAsync(Guid id);
    }
}
