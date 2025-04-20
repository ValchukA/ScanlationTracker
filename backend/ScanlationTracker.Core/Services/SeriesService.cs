using Microsoft.Extensions.Logging;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Scrapers;

namespace ScanlationTracker.Core.Services;

internal class SeriesService : ISeriesService
{
    private readonly ISeriesRepository _seriesRepository;
    private readonly IScanlationScraperFactory _scraperFactory;
    private readonly ILogger<SeriesService> _logger;

    public SeriesService(
        ISeriesRepository seriesRepository,
        IScanlationScraperFactory scraperFactory,
        ILogger<SeriesService> logger)
    {
        _seriesRepository = seriesRepository;
        _scraperFactory = scraperFactory;
        _logger = logger;
    }

    public async Task UpdateSeriesAsync()
    {
        var groups = await _seriesRepository.GetAllGroupsAsync();

        foreach (var group in groups)
        {
            var scraper = _scraperFactory.CreateScraper(group.Name, group.BaseWebsiteUrl);

            await foreach (var seriesUpdate in scraper.ScrapeLatestUpdatesAsync())
            {
                var series = await scraper.ScrapeSeriesAsync(seriesUpdate.SeriesUrl);
                var normalizedSeriesTitle = NormalizeWhitespaces(series.Title);

                foreach (var chapter in series.LatestChapters)
                {
                    _logger.LogInformation(
                        "{SeriesTitle} - {CoverUrl} - {ChapterUrl} - {ChapterTitle}",
                        normalizedSeriesTitle,
                        series.CoverUrl,
                        chapter.Url,
                        NormalizeWhitespaces(chapter.Title));
                }
            }
        }

        static string NormalizeWhitespaces(string str)
            => string.Join(' ', str.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
