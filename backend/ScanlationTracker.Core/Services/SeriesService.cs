using Microsoft.Extensions.Logging;
using ScanlationTracker.Core.Metrics;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.UrlManagers;
using System.Diagnostics;

namespace ScanlationTracker.Core.Services;

internal class SeriesService : ISeriesService
{
    private readonly ISeriesRepository _seriesRepository;
    private readonly IUrlManagerFactory _urlManagerFactory;
    private readonly IScanlationScraperFactory _scraperFactory;
    private readonly ILogger<SeriesService> _logger;
    private readonly CoreMetrics _coreMetrics;

    public SeriesService(
        ISeriesRepository seriesRepository,
        IUrlManagerFactory urlManagerFactory,
        IScanlationScraperFactory scraperFactory,
        ILogger<SeriesService> logger,
        CoreMetrics coreMetrics)
    {
        _seriesRepository = seriesRepository;
        _urlManagerFactory = urlManagerFactory;
        _scraperFactory = scraperFactory;
        _logger = logger;
        _coreMetrics = coreMetrics;
    }

    public async Task UpdateSeriesAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var groups = await _seriesRepository.GetAllGroupsAsync();

        await Parallel.ForEachAsync(groups, async (group, _) =>
        {
            using var scope = _logger.BeginScope("{GroupName}", group.Name);

            try
            {
                var urlManager = _urlManagerFactory.CreateUrlManager(
                    group.Name,
                    group.BaseWebsiteUrl,
                    group.BaseCoverUrl);
                var scraper = _scraperFactory.CreateScraper(group.Name, urlManager.LatestUpdatesUrl);

                await foreach (var seriesUpdate in scraper.ScrapeLatestUpdatesAsync())
                {
                    var seriesId = urlManager.ExtractSeriesId(seriesUpdate.SeriesUrl);
                    await using var series = await scraper.ScrapeSeriesAsync(seriesUpdate.SeriesUrl);
                    var normalizedSeriesTitle = NormalizeWhitespaces(series.Title);
                    var relativeCoverUrl = urlManager.ExtractRelativeCoverUrl(series.CoverUrl);

                    await foreach (var chapter in series.LatestChaptersAsync)
                    {
                        _logger.LogInformation(
                            "{SeriesId} - {SeriesUrl} - {SeriesTitle} - {CoverUrl} - {RelativeCoverUrl} - " +
                            "{ChapterId} - {ChapterUrl} - {ChapterTitle}",
                            seriesId,
                            seriesUpdate.SeriesUrl,
                            normalizedSeriesTitle,
                            series.CoverUrl,
                            relativeCoverUrl,
                            urlManager.ExtractChapterId(chapter.Url),
                            chapter.Url,
                            NormalizeWhitespaces(chapter.Title));
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to update series");
            }
        });

        var elapsed = stopwatch.Elapsed;
        _coreMetrics.AddSeriesUpdateDuration(elapsed);
        _logger.LogInformation("Series update took {ElapsedSeconds} s", elapsed.TotalSeconds);

        static string NormalizeWhitespaces(string str)
            => string.Join(' ', str.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
