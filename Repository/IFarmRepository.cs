using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Repository
{
    public interface IFarmRepository
    {
        Task<Farm?> GetFarmByIdAsync(Guid id);
        Task<Farm> CreateFarmAsync(Farm farm);
        Task<Farm?> UpdateFarmAsync(Guid id, Farm farm);
        Task<Farm?> DeleteFarmAsync(Guid id);
        Task<List<Farm>> GetFarmsByUserAsync(Guid userId, string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = false, int pageNumber = 1, int pageSize=1000);
    }
}