using BarnManagementApi.Models.Domain;
using BarnManagementApi.Repository;
using BarnManagementApi.Services.Interfaces;

namespace BarnManagementApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<User> RegisterUserAsync(string username, string password)
        {
            var existing = await userRepository.GetByUsernameAsync(username);
            if (existing != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                // NOTE: hashing is out-of-scope; store as-is for now
                PasswordHash = password,
                Balance = 1000,
                CreatedAt = DateTime.UtcNow
            };

            return await userRepository.CreateAsync(user);
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
        {
            var user = await userRepository.GetByUsernameAsync(username);
            if (user == null || user.PasswordHash != password)
            {
                return null;
            }
            // Stub: return fake token
            return $"FAKE-JWT-TOKEN::{user.Id}";
        }

        public Task<User?> GetUserByIdAsync(Guid userId)
        {
            return userRepository.GetByIdAsync(userId);
        }

        public async Task<User?> UpdateUserAsync(Guid userId, User newData)
        {
            var existing = await userRepository.GetByIdAsync(userId);
            if (existing == null) return null;

            existing.Username = string.IsNullOrWhiteSpace(newData.Username) ? existing.Username : newData.Username;
            if (!string.IsNullOrWhiteSpace(newData.PasswordHash))
            {
                existing.PasswordHash = newData.PasswordHash;
            }
            existing.UpdatedAt = DateTime.UtcNow;
            return await userRepository.UpdateAsync(existing);
        }

        public Task<User?> AdjustBalanceAsync(Guid userId, decimal amount)
        {
            return userRepository.AdjustBalanceAsync(userId, amount);
        }
    }
}


