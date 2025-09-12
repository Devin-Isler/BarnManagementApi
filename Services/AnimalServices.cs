using BarnManagementApi.Data;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementApi.Services
{
    public class AnimalServices : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(20);
        private readonly ILogger<AnimalServices>? logger;

        public AnimalServices(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var log = scope.ServiceProvider.GetService<ILogger<AnimalServices>>();
                    log?.LogDebug("AnimalServices tick started at {UtcNow}", DateTime.UtcNow);
                }
                await MarkExpiredAsDeadAsync(stoppingToken);
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

        private async Task MarkExpiredAsDeadAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BarnDbContext>();
            var log = scope.ServiceProvider.GetService<ILogger<AnimalServices>>();
            var now = DateTime.UtcNow;
            var affected = await db.Animals
                .Where(a => a.IsActive && a.DeathTime != null && a.DeathTime <= now)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false), cancellationToken);
            log?.LogInformation("Marked {Count} animals as inactive due to death time passed", affected);
        }
    }
}
