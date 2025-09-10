using BarnManagementApi.Models.Domain;
using BarnManagementApi.Repository;
using BarnManagementApi.Services.Interfaces;

namespace BarnManagementApi.Services
{
    public class AnimalService : IAnimalService
    {
        private readonly IAnimalRepository animalRepository;
        private readonly IFarmRepository farmRepository;
        private readonly IUserRepository userRepository;
        private readonly IProductRepository productRepository;

        public AnimalService(
            IAnimalRepository animalRepository,
            IFarmRepository farmRepository,
            IUserRepository userRepository,
            IProductRepository productRepository)
        {
            this.animalRepository = animalRepository;
            this.farmRepository = farmRepository;
            this.userRepository = userRepository;
            this.productRepository = productRepository;
        }

        public async Task<Animal?> BuyAnimalAsync(Guid userId, Guid farmId, Animal animal)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");
            var farm = await farmRepository.GetFarmByIdAsync(farmId);
            if (farm == null) throw new KeyNotFoundException("Farm not found.");

            if (user.Balance < animal.PurchasePrice)
            {
                throw new InvalidOperationException("Insufficient balance.");
            }

            user = await userRepository.AdjustBalanceAsync(userId, -animal.PurchasePrice)
                   ?? throw new InvalidOperationException("Failed to adjust balance.");

            animal.Id = Guid.NewGuid();
            animal.FarmId = farmId;
            animal.CreatedAt = DateTime.UtcNow;
            return await animalRepository.CreateAnimalAsync(animal);
        }

        public async Task<Animal?> SellAnimalAsync(Guid userId, Guid animalId)
        {
            var animal = await animalRepository.GetAnimalByIdAsync(animalId);
            if (animal == null) return null;

            var user = await userRepository.AdjustBalanceAsync(userId, animal.SellPrice);
            if (user == null) throw new InvalidOperationException("Failed to adjust balance.");

            return await animalRepository.DeleteAnimalAsync(animalId);
        }

        public async Task<Product?> ProduceProductAsync(Guid animalId)
        {
            var animal = await animalRepository.GetAnimalByIdAsync(animalId);
            if (animal == null) return null;

            var now = DateTime.UtcNow;
            if (animal.DeathTime != null) return null;
            if (animal.LastProductionTime != null &&
                animal.LastProductionTime.Value.AddMinutes(animal.ProductionInterval) > now)
            {
                // Too soon to produce again
                return null;
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = $"{animal.Name} Product",
                Price = animal.SellPrice, // simple rule; adjust as needed
                AnimalId = animal.Id,
                CreatedAt = now
            };
            animal.LastProductionTime = now;
            await animalRepository.UpdateAnimalAsync(animal.Id, animal);
            return await productRepository.CreateProductAsync(product);
        }

        public async Task<Animal?> CheckAnimalLifetimeAsync(Guid animalId)
        {
            var animal = await animalRepository.GetAnimalByIdAsync(animalId);
            if (animal == null) return null;
            var deathAt = animal.CreatedAt.AddHours(animal.Lifetime);
            if (DateTime.UtcNow >= deathAt)
            {
                animal.DeathTime = deathAt;
                return await animalRepository.UpdateAnimalAsync(animal.Id, animal);
            }
            return animal;
        }
    }
}


