using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Services.Interfaces
{
    public interface IFarmService
    {
        Task<Farm> CreateFarmAsync(Guid userId, string farmName, string? description, string? location);
        Task<Farm?> GetFarmByIdAsync(Guid farmId);
        Task<List<Farm>> GetFarmsByUserAsync(Guid userId);
        Task<Farm?> UpdateFarmAsync(Guid farmId, Farm newData);
        Task<Farm?> DeleteFarmAsync(Guid farmId);
    }
}


