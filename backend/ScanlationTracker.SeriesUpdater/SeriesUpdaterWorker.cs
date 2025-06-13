using Microsoft.Extensions.Options;
using ScanlationTracker.Core.Services;

namespace ScanlationTracker.SeriesUpdater;

internal class SeriesUpdaterWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly PeriodicTimer _timer;
    private readonly ILogger<SeriesUpdaterWorker> _logger;

    public SeriesUpdaterWorker(
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<SeriesUpdaterSettings> settings,
        ILogger<SeriesUpdaterWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _timer = new PeriodicTimer(GetPeriod());
        _logger = logger;

        settings.OnChange(_ =>
        {
            _timer.Period = GetPeriod();

            _logger.LogInformation(
                "Series update interval changed to {Minutes} minute(s)",
                settings.CurrentValue.UpdateIntervalInMinutes);
        });

        TimeSpan GetPeriod() => TimeSpan.FromMinutes(settings.CurrentValue.UpdateIntervalInMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var seriesService = scope.ServiceProvider.GetRequiredService<ISeriesService>();

        do
        {
            await seriesService.UpdateSeriesAsync();
        }
        while (await _timer.WaitForNextTickAsync(stoppingToken));
    }
}
