using Microsoft.Extensions.Logging;
using ScanlationTracker.Core.Metrics;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Repositories.Dtos;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.Scrapers.Dtos;
using ScanlationTracker.Core.UrlManagers;
using System.Diagnostics;

namespace ScanlationTracker.Core.Services;

internal class SeriesService : ISeriesService
{
    private readonly ISeriesRepositoryFactory _seriesRepositoryFactory;
    private readonly IUrlManagerFactory _urlManagerFactory;
    private readonly IScanlationScraperFactory _scraperFactory;
    private readonly ILogger<SeriesService> _logger;
    private readonly CoreMetrics _coreMetrics;

    public SeriesService(
        ISeriesRepositoryFactory seriesRepositoryFactory,
        IUrlManagerFactory urlManagerFactory,
        IScanlationScraperFactory scraperFactory,
        ILogger<SeriesService> logger,
        CoreMetrics coreMetrics)
    {
        _seriesRepositoryFactory = seriesRepositoryFactory;
        _urlManagerFactory = urlManagerFactory;
        _scraperFactory = scraperFactory;
        _logger = logger;
        _coreMetrics = coreMetrics;
    }

    public async Task UpdateSeriesAsync()
    {
        var updateDate = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var groups = await _seriesRepositoryFactory.CreateRepository().GetAllGroupsAsync();

        await Parallel.ForEachAsync(groups, async (group, _) =>
        {
            using var scope = _logger.BeginScope("{GroupName}", group.Name);

            try
            {
                await UpdateSeriesFromGroupAsync(group, updateDate);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to update series");
            }
        });

        var elapsed = stopwatch.Elapsed;
        _coreMetrics.AddSeriesUpdateDuration(elapsed);
        _logger.LogInformation("Series update took {ElapsedSeconds} s", elapsed.TotalSeconds);
    }

    private static string NormalizeWhitespaces(string str)
        => string.Join(' ', str.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private async Task UpdateSeriesFromGroupAsync(ScanlationGroupDto group, DateTimeOffset updateDate)
    {
        var urlManager = _urlManagerFactory.CreateUrlManager(
                    group.Name,
                    group.BaseWebsiteUrl,
                    group.BaseCoverUrl);
        var scraper = _scraperFactory.CreateScraper(group.Name, urlManager.LatestUpdatesUrl);
        var seriesRepository = _seriesRepositoryFactory.CreateRepository();

        await foreach (var seriesUpdate in scraper.ScrapeLatestUpdatesAsync())
        {
            var seriesExternalId = urlManager.ExtractSeriesId(seriesUpdate.SeriesUrl);
            var series = await seriesRepository.GetSeriesAsync(group.Id, seriesExternalId);

            if (series is not null)
            {
                var latestChapterExternalId =
                    urlManager.ExtractChapterId(seriesUpdate.LatestChapterUrl);
                var latestSavedChapter = await seriesRepository.GetLatestChapterAsync(series.Id);

                if (latestChapterExternalId == latestSavedChapter.ExternalId)
                {
                    _logger.LogInformation(
                        "Latest chapter for series with Id {SeriesId} and external Id " +
                        "{SeriesExternalId} is already saved. Remaining series are up to date",
                        series.Id,
                        series.ExternalId);

                    break;
                }

                await UpdateSingleSeriesAsync(
                    scraper,
                    urlManager,
                    seriesRepository,
                    series,
                    seriesUpdate.SeriesUrl,
                    latestSavedChapter,
                    updateDate,
                    group.Name);
            }
            else
            {
                await AddSeriesAsync(
                    scraper,
                    urlManager,
                    seriesRepository,
                    group,
                    seriesExternalId,
                    seriesUpdate.SeriesUrl,
                    updateDate);
            }
        }

        await seriesRepository.SaveChangesAsync();
    }

    private async Task AddSeriesAsync(
        IScanlationScraper scraper,
        IUrlManager urlManager,
        ISeriesRepository seriesRepository,
        ScanlationGroupDto group,
        string seriesExternalId,
        string seriesUrl,
        DateTimeOffset updateDate)
    {
        await using var scrapedSeries = await scraper.ScrapeSeriesAsync(seriesUrl);
        var series = new SeriesDto
        {
            Id = Guid.CreateVersion7(),
            ScanlationGroupId = group.Id,
            ExternalId = seriesExternalId,
            Title = NormalizeWhitespaces(scrapedSeries.Title),
            RelativeCoverUrl = urlManager.ExtractRelativeCoverUrl(scrapedSeries.CoverUrl),
        };

        seriesRepository.AddSeries(series);

        _coreMetrics.IncrementAddedSeriesCounter(group.Name);
        _logger.LogInformation(
            "Added series with Id {SeriesId} and external Id {SeriesExternalId}",
            series.Id,
            series.ExternalId);

        await AddChaptersAsync(
            urlManager,
            seriesRepository,
            scrapedSeries,
            series,
            updateDate,
            group.Name);
    }

    private async Task UpdateSingleSeriesAsync(
        IScanlationScraper scraper,
        IUrlManager urlManager,
        ISeriesRepository seriesRepository,
        SeriesDto series,
        string seriesUrl,
        ChapterDto latestSavedChapter,
        DateTimeOffset updateDate,
        ScanlationGroupName groupName)
    {
        await using var scrapedSeries = await scraper.ScrapeSeriesAsync(seriesUrl);
        var relativeCoverUrl = urlManager.ExtractRelativeCoverUrl(scrapedSeries.CoverUrl);

        if (series.RelativeCoverUrl != relativeCoverUrl)
        {
            seriesRepository.UpdateSeries(series with { RelativeCoverUrl = relativeCoverUrl });

            _logger.LogInformation(
                "Updated cover URL for series with Id {SeriesId} and external Id {SeriesExternalId}",
                series.Id,
                series.ExternalId);
        }

        await AddChaptersAsync(
            urlManager,
            seriesRepository,
            scrapedSeries,
            series,
            updateDate,
            groupName,
            latestSavedChapter);
    }

    private async Task AddChaptersAsync(
        IUrlManager urlManager,
        ISeriesRepository seriesRepository,
        IScrapedSeries scrapedSeries,
        SeriesDto series,
        DateTimeOffset updateDate,
        ScanlationGroupName groupName,
        ChapterDto? latestSavedChapter = null)
    {
        var chaptersToSave = new List<(string Title, string ExternalId)>();

        await foreach (var chapter in scrapedSeries.LatestChaptersAsync)
        {
            var chapterExternalId = urlManager.ExtractChapterId(chapter.Url);

            if (chapterExternalId == latestSavedChapter?.ExternalId)
            {
                break;
            }

            chaptersToSave.Add((chapter.Title, chapterExternalId));
        }

        var previousChapterNumber = latestSavedChapter?.Number ?? 0;

        for (var index = chaptersToSave.Count - 1; index >= 0; index--)
        {
            var chapter = new ChapterDto
            {
                Id = Guid.CreateVersion7(),
                SeriesId = series.Id,
                ExternalId = chaptersToSave[index].ExternalId,
                Title = NormalizeWhitespaces(chaptersToSave[index].Title),
                Number = ++previousChapterNumber,
                AddedAt = updateDate,
            };

            seriesRepository.AddChapter(chapter);

            _coreMetrics.IncrementAddedChaptersCounter(groupName);
            _logger.LogInformation(
                "Added chapter with Id {ChapterId} and external Id {ChapterExternalId} " +
                "to series with Id {SeriesId} and external Id {SeriesExternalId}",
                chapter.Id,
                chapter.ExternalId,
                series.Id,
                series.ExternalId);
        }
    }
}
