using BarnManagementApi.Models.Domain;
using BarnManagementApi.Repository;
using BarnManagementApi.Services.Interfaces;

namespace BarnManagementApi.Services
{
    public class FarmService : IFarmService
    {
        private readonly IFarmRepository farmRepository;
        private readonly IUserRepository userRepository;

        public FarmService(IFarmRepository farmRepository, IUserRepository userRepository)
        {
            this.farmRepository = farmRepository;
            this.userRepository = userRepository;
        }

        public async Task<Farm> CreateFarmAsync(Guid userId, string farmName, string? description, string? location)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            var farm = new Farm
            {
                Id = Guid.NewGuid(),
                Name = farmName,
                Description = description,
                Location = location,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };
            return await farmRepository.CreateFarmAsync(farm);
        }

        public Task<Farm?> GetFarmByIdAsync(Guid farmId)
        {
            return farmRepository.GetFarmByIdAsync(farmId);
        }

        public Task<List<Farm>> GetFarmsByUserAsync(Guid userId)
        {
            return farmRepository.GetFarmsByUserAsync(userId);
        }

        public async Task<Farm?> UpdateFarmAsync(Guid farmId, Farm newData)
        {
            var existing = await farmRepository.GetFarmByIdAsync(farmId);
            if (existing == null) return null;
            existing.Name = string.IsNullOrWhiteSpace(newData.Name) ? existing.Name : newData.Name;
            existing.Description = newData.Description ?? existing.Description;
            existing.Location = newData.Location ?? existing.Location;
            existing.LastUpdatedAt = DateTime.UtcNow;
            return await farmRepository.UpdateFarmAsync(farmId, existing);
        }

        public Task<Farm?> DeleteFarmAsync(Guid farmId)
        {
            return farmRepository.DeleteFarmAsync(farmId);
        }
    }
}


