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
                
                // Identify animals whose death time has passed
                var expiredIds = await MarkExpiredAsDeadAsync(stoppingToken);

                // Persist changes here (keep detection and persistence separate)
                if (expiredIds.Count > 0)
                {
                    using var scope = serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<BarnDbContext>();
                    var log = scope.ServiceProvider.GetService<ILogger<AnimalServices>>();

                    var affected = await db.Animals
                        .Where(a => expiredIds.Contains(a.Id))
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false), stoppingToken);

                    if (affected > 0)
                    {
                        log?.LogInformation("Marked {Count} animals as inactive due to death time passed", affected);
                    }
                }
                
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

        // Returns the IDs of animals whose death time has passed and are currently active
        public async Task<List<Guid>> MarkExpiredAsDeadAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BarnDbContext>();
            var now = DateTime.UtcNow;
            
            // Identify animals that should be marked inactive (no persistence here)
            var ids = await db.Animals
                .Where(a => a.IsActive && a.DeathTime != null && a.DeathTime <= now)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            return ids;
        }
    }
}
