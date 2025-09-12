using BarnManagementApi.Data;
using BarnManagementApi.Repository;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Services
{
    public class ProductService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(20);
        private readonly ILogger<ProductService>? logger;

        public ProductService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            // ILogger cannot be injected directly here without changing DI registration; resolve lazily when needed
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var log = scope.ServiceProvider.GetService<ILogger<ProductService>>();
                    log?.LogDebug("ProductService tick started at {UtcNow}", DateTime.UtcNow);
                    await ProduceDueProductsAsync(stoppingToken);
                    log?.LogDebug("ProductService tick completed at {UtcNow}", DateTime.UtcNow);
                }
                catch
                {
                    using var scope = serviceProvider.CreateScope();
                    var log = scope.ServiceProvider.GetService<ILogger<ProductService>>();
                    log?.LogError("ProduceDueProductsAsync threw an exception; continuing next tick");
                }

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

        private async Task ProduceDueProductsAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BarnDbContext>();
            var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            var log = scope.ServiceProvider.GetService<ILogger<ProductService>>();

            var now = DateTime.UtcNow;

            const int batchSize = 100;

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

            await db.SaveChangesAsync(cancellationToken);
            log?.LogInformation("Saved production changes for {Count} animals", dueAnimals.Count);
        }
    }
}