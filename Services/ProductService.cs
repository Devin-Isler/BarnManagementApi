using BarnManagementApi.Models.Domain;
using BarnManagementApi.Repository;
using BarnManagementApi.Services.Interfaces;

namespace BarnManagementApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository productRepository;
        private readonly IUserRepository userRepository;

        public ProductService(IProductRepository productRepository, IUserRepository userRepository)
        {
            this.productRepository = productRepository;
            this.userRepository = userRepository;
        }

        public Task<Product?> GetProductByIdAsync(Guid productId)
        {
            return productRepository.GetProductByIdAsync(productId);
        }

        public Task<List<Product>> GetProductsByAnimalAsync(Guid animalId)
        {
            return productRepository.GetProductsByAnimalAsync(animalId);
        }

        public async Task<Product?> SellProductAsync(Guid userId, Guid productId)
        {
            var product = await productRepository.GetProductByIdAsync(productId);
            if (product == null) return null;
            if (product.SoldAt != null) return product;

            var user = await userRepository.AdjustBalanceAsync(userId, product.Price);
            if (user == null) throw new InvalidOperationException("Failed to adjust balance.");

            product.SoldAt = DateTime.UtcNow;
            return await productRepository.UpdateProductAsync(productId, product);
        }

        public Task<List<Product>> GetAvailableProductsAsync(Guid farmId)
        {
            return productRepository.GetAvailableProductsByFarmAsync(farmId);
        }
    }
}


