using Microsoft.Extensions.Options;
using ScanlationTracker.Core.Services.Interfaces;

namespace ScanlationTracker.SeriesUpdater;

internal class SeriesUpdaterWorker : BackgroundService
{
    private readonly ISeriesUpdaterService _seriesUpdaterService;
    private readonly PeriodicTimer _timer;
    private readonly ILogger<SeriesUpdaterWorker> _logger;

    public SeriesUpdaterWorker(
        ISeriesUpdaterService seriesUpdaterService,
        IOptionsMonitor<SeriesUpdaterSettings> settings,
        ILogger<SeriesUpdaterWorker> logger)
    {
        _seriesUpdaterService = seriesUpdaterService;
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
            await _seriesUpdaterService.UpdateSeriesAsync();
        }
        while (await _timer.WaitForNextTickAsync(stoppingToken));
    }
}
