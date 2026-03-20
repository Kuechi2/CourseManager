// Datei: Services/Maintenance/DailyMaintenanceBackgroundService.cs
public class DailyMaintenanceBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DailyMaintenanceBackgroundService> _logger;

    public DailyMaintenanceBackgroundService(IServiceProvider services, ILogger<DailyMaintenanceBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Daily Maintenance started...");

            using (var scope = _services.CreateScope())
            {
                var statsService = scope.ServiceProvider.GetRequiredService<SchoolStatsService>();
                await statsService.UpdateGlobalAveragesAsync();
            }

            // Warte 24 Stunden (oder berechne die Zeit bis 03:00 Uhr nachts)
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}