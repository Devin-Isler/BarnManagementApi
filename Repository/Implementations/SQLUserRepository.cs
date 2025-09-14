// User Repository Implementation - Defines data access operations for users
// Provides abstraction layer for user-related database operations

using BarnManagementApi.Data;
using BarnManagementApi.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Repository
{
    public class SQLUserRepository : IUserRepository
    {
        // Dependencies for animal operations
        private readonly BarnDbContext context; // Database access

        public SQLUserRepository(BarnDbContext context)
        {
            this.context = context;
        }

        // Get user by ID
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        // Create a new user
        public async Task<User> CreateAsync(User user)
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        }

        // Update existing user details
        public async Task<User?> UpdateAsync(User user)
        {
            var existing = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existing == null) return null;

            existing.Username = user.Username;
            existing.PasswordHash = user.PasswordHash;
            existing.Balance = user.Balance;
            existing.UpdatedAt = DateTime.UtcNow; // Update timestamp

            await context.SaveChangesAsync();
            return existing;
        }

        // Set user balance directly
        public async Task<User?> SetBalanceAsync(Guid userId, decimal amount)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            user.Balance = amount;
            user.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return user;
        }

        // Delete a user along with all farms, animals, and products
        public async Task<(bool success, int farmsCount, int animalsCount, int productsCount)> DeleteUserAsync(Guid userId)
        {
            // Get all farms for this user
            var farms = await context.Farms.Where(f => f.UserId == userId).ToListAsync();
            var farmIds = farms.Select(f => f.Id).ToList();
            var farmsCount = farms.Count;

            var animalsCount = 0;
            var productsCount = 0;

            if (farmIds.Count > 0)
            {
                // Get all animals for these farms
                var animals = await context.Animals.Where(a => farmIds.Contains(a.FarmId)).ToListAsync();
                var animalIds = animals.Select(a => a.Id).ToList();
                animalsCount = animals.Count;

                if (animalIds.Count > 0)
                {
                    // Delete all products for these animals
                    var products = await context.Products.Where(p => animalIds.Contains(p.AnimalId)).ToListAsync();
                    productsCount = products.Count;
                    if (products.Count > 0)
                    {
                        context.Products.RemoveRange(products);
                    }

                    // Delete all animals
                    context.Animals.RemoveRange(animals);
                }

                // Delete all farms
                context.Farms.RemoveRange(farms);
            }

            // Delete the user
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                context.Users.Remove(user);
            }

            await context.SaveChangesAsync();
            return (true, farmsCount, animalsCount, productsCount);
        }
    }
}