using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Repository
{
    public interface IFarmRepository
    {
        Task<List<Farm>> GetAllFarmsAsync();
        Task<Farm?> GetFarmByIdAsync(Guid id);
        Task<Farm> CreateFarmAsync(Farm farm);
        Task<Farm?> UpdateFarmAsync(Guid id, Farm farm);
        Task<Farm?> DeleteFarmAsync(Guid id);
        Task<List<Farm>> GetFarmsByUserAsync(Guid userId);
    }
}