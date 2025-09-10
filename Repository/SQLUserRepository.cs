using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Repository
{
    public class SQLUserRepository : IUserRepository
    {
        private readonly BarnDbContext context;

        public SQLUserRepository(BarnDbContext context)
        {
            this.context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> CreateAsync(User user)
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateAsync(User user)
        {
            var existing = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existing == null) return null;

            existing.Username = user.Username;
            existing.PasswordHash = user.PasswordHash;
            existing.Balance = user.Balance;
            existing.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<User?> AdjustBalanceAsync(Guid userId, decimal amount)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;
            user.Balance += amount;
            user.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return user;
        }
    }
}


