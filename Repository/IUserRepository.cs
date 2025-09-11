using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User> CreateAsync(User user);
        Task<User?> UpdateAsync(User user);
        Task<User?> AdjustBalanceAsync(Guid userId, decimal amount);
        Task<bool> DeleteUserAsync(Guid userId);
    }
}


