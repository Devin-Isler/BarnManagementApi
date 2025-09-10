using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(string username, string password);
        Task<string?> AuthenticateAsync(string username, string password);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> UpdateUserAsync(Guid userId, User newData);
        Task<User?> AdjustBalanceAsync(Guid userId, decimal amount);
    }
}


