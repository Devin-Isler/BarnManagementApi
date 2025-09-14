// User Repository Interface - Defines data access operations for Users
// Provides abstraction layer for User-related database operations
using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Repository
{
    public interface IUserRepository
    {
        // Get the registered user 
        Task<User?> GetByIdAsync(Guid id);

        // Create the user
        Task<User> CreateAsync(User user);

        // Update user information
        Task<User?> UpdateAsync(User user);

        // Set the balance
        Task<User?> SetBalanceAsync(Guid userId, decimal amount);

        // Hard delete the user
        Task<(bool success, int farmsCount, int animalsCount, int productsCount)> DeleteUserAsync(Guid userId);
    }
}


