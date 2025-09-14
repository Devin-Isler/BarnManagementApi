// Animal Services - Background service for managing animal lifecycle events
// Handles automatic animal death and lifecycle management

using BarnManagementApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BarnManagementApi.Services
{
    public class AnimalServices : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(20); // Check every 20 seconds
        private readonly ILogger<AnimalServices>? logger;

        public AnimalServices(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        // Main background service execution loop
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Log service tick start
                using (var scope = serviceProvider.CreateScope())
                {
                    var log = scope.ServiceProvider.GetService<ILogger<AnimalServices>>();
                    log?.LogDebug("AnimalServices tick started at {UtcNow}", DateTime.UtcNow);
                }
                
                // Check for animals that should be marked as dead
                await MarkExpiredAsDeadAsync(stoppingToken);
                
                // Wait for next check interval
                try
                {
                    await Task.Delay(pollInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    using var scope = serviceProvider.CreateScope();
                    var log = scope.ServiceProvider.GetService<ILogger<AnimalServices>>();
                    log?.LogInformation("AnimalServices is stopping");
                }
            }
        }

        // Mark animals as dead when their death time has passed
        private async Task MarkExpiredAsDeadAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BarnDbContext>();
            var log = scope.ServiceProvider.GetService<ILogger<AnimalServices>>();
            var now = DateTime.UtcNow;
            
            // Find animals that should be marked as dead
            var affected = await db.Animals
                .Where(a => a.IsActive && a.DeathTime != null && a.DeathTime <= now)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false), cancellationToken);
                
            if (affected > 0) // Check if any animals were marked as dead
            {
                log?.LogInformation("Marked {Count} animals as inactive due to death time passed", affected);
            }
        }
    }
}
