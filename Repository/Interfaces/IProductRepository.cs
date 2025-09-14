// Product Repository Interface - Defines data access operations for products
// Provides abstraction layer for product-related database operations
using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Repository
{
    public interface IProductRepository
    {
        // Get all animals for a user with filtering, sorting and pagination
        Task<List<Product>> GetAllProductsAsync(Guid userId, string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = false, int pageNumber = 1, int pageSize=1000);

        // Get a specific animal by ID
        Task<Product?> GetProductByIdAsync(Guid id);

        // Produce product
        Task<Product> ProduceProductAsync(Guid animalId);

        // Update an existing product
        Task<Product?> UpdateProductAsync(Guid id, Product product);

        // Hard delete a product from database
        Task<Product?> DeleteProductAsync(Guid id);

        // Sell a product, soft delete, update user balance
        Task<Product?> SellProductAsync(Guid id);
    }
}
