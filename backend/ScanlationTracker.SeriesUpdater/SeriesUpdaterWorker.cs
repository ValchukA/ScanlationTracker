using Microsoft.Extensions.Options;
using ScanlationTracker.Core.Services;

namespace ScanlationTracker.SeriesUpdater;

internal class SeriesUpdaterWorker : BackgroundService
{
    private readonly ISeriesService _seriesService;
    private readonly PeriodicTimer _timer;
    private readonly ILogger<SeriesUpdaterWorker> _logger;

    public SeriesUpdaterWorker(
        ISeriesService seriesService,
        IOptionsMonitor<SeriesUpdaterSettings> settings,
        ILogger<SeriesUpdaterWorker> logger)
    {
        _seriesService = seriesService;
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
        do
        {
            await _seriesService.UpdateSeriesAsync();
        }
        while (await _timer.WaitForNextTickAsync(stoppingToken));
    }
}
