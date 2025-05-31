using Microsoft.Extensions.Logging;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.UrlManagers;

namespace ScanlationTracker.Core.Services;

internal class SeriesService : ISeriesService
{
    private readonly ISeriesRepository _seriesRepository;
    private readonly IUrlManagerFactory _urlManagerFactory;
    private readonly IScanlationScraperFactory _scraperFactory;
    private readonly ILogger<SeriesService> _logger;

    public SeriesService(
        ISeriesRepository seriesRepository,
        IUrlManagerFactory urlManagerFactory,
        IScanlationScraperFactory scraperFactory,
        ILogger<SeriesService> logger)
    {
        _seriesRepository = seriesRepository;
        _urlManagerFactory = urlManagerFactory;
        _scraperFactory = scraperFactory;
        _logger = logger;
    }

    public async Task UpdateSeriesAsync()
    {
        var groups = await _seriesRepository.GetAllGroupsAsync();

        await Parallel.ForEachAsync(groups, async (group, _) =>
        {
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
                _logger.LogError(exception, "Failed to update series from {GroupName}", group.Name);
            }
        });

        static string NormalizeWhitespaces(string str)
            => string.Join(' ', str.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
