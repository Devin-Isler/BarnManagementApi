// Product Service - Background service for automatic product generation
// Handles automatic product creation from animals based on production intervals

using BarnManagementApi.Data;
using BarnManagementApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BarnManagementApi.Services
{
    public class ProductService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(20); // Check every 20 seconds

        public ProductService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            // ILogger cannot be injected directly here without changing DI registration; resolve lazily when needed
        }

        // Main background service execution loop
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var log = scope.ServiceProvider.GetService<ILogger<ProductService>>();
                    log?.LogDebug("ProductService tick started at {UtcNow}", DateTime.UtcNow);
                    
                    // Generate products for animals that are due
                    await ProduceDueProductsAsync(stoppingToken);
                    
                    log?.LogDebug("ProductService tick completed at {UtcNow}", DateTime.UtcNow);
                }
                catch
                {
                    using var scope = serviceProvider.CreateScope();
                    var log = scope.ServiceProvider.GetService<ILogger<ProductService>>();
                    log?.LogError("ProduceDueProductsAsync threw an exception; continuing next tick");
                }

                // Wait for next check interval
                try
                {
                    await Task.Delay(pollInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    using var scope = serviceProvider.CreateScope();
                    var log = scope.ServiceProvider.GetService<ILogger<ProductService>>();
                    log?.LogInformation("ProductService is stopping");
                }
            }
        }

        // Generate products for animals that are due for production
        public async Task ProduceDueProductsAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BarnDbContext>();
            var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            var log = scope.ServiceProvider.GetService<ILogger<ProductService>>();

            var now = DateTime.UtcNow;
            const int batchSize = 100; // Process animals in batches to avoid overwhelming the database

            // Find animals that are due to produce products
            var dueAnimals = await db.Animals
                .Where(a => a.IsActive)
                .Where(a =>
                    (a.LastProductionTime == null && a.CreatedAt.AddMinutes(a.ProductionInterval) <= now) ||
                    (a.LastProductionTime != null && a.LastProductionTime.Value.AddMinutes(a.ProductionInterval) <= now))
                .OrderBy(a => a.LastProductionTime ?? a.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (dueAnimals.Count == 0)
            {
                log?.LogDebug("No due animals for production at {Now}", now);
                return;
            }

            log?.LogInformation("Producing products for {Count} animals", dueAnimals.Count);
            
            // Generate products for each due animal
            foreach (var animal in dueAnimals)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await productRepo.ProduceProductAsync(animal.Id);
                    animal.LastProductionTime = now;
                    log?.LogInformation("Produced product for Animal {AnimalId}", animal.Id);
                }
                catch
                {
                    log?.LogWarning("Failed to produce product for Animal {AnimalId}; continuing", animal.Id);
                }
            }

            // Save all changes to database
            await db.SaveChangesAsync(cancellationToken);
            log?.LogInformation("Saved production changes for {Count} animals", dueAnimals.Count);
        }
    }
}