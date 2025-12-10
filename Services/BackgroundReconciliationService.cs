using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SIOMS.Services
{
    public class BackgroundReconciliationService : BackgroundService
    {
        private readonly ILogger<BackgroundReconciliationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;
        
        public BackgroundReconciliationService(ILogger<BackgroundReconciliationService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Reconciliation Service is starting.");
            
            // Run daily at 2:00 AM
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0);
            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }
            
            var initialDelay = nextRun - now;
            
            _timer = new Timer(DoWork, null, initialDelay, TimeSpan.FromDays(1));
            
            return Task.CompletedTask;
        }
        
        private async void DoWork(object? state)
        {
            _logger.LogInformation("Starting daily stock reconciliation...");
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
                
                try
                {
                    await inventoryService.DailyStockReconciliationAsync();
                    _logger.LogInformation("Daily stock reconciliation completed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during daily stock reconciliation.");
                }
            }
        }
        
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Reconciliation Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return base.StopAsync(stoppingToken);
        }
    }
}