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
        var stopwatch = Stopwatch.StartNew();
        var updateDate = DateTimeOffset.UtcNow;
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

    private static async Task<(SeriesDto? SavedSeries, IScrapedSeries? ScrapedSeries)> GetSeriesAsync(
        ISeriesRepository seriesRepository,
        IScanlationScraper scraper,
        ScanlationGroupDto group,
        string seriesExternalId,
        string seriesUrl)
    {
        var savedSeries = await seriesRepository.GetSeriesByExternalIdAsync(
            group.Id,
            seriesExternalId);
        IScrapedSeries? scrapedSeries = null;

        if (savedSeries is null && group.Name == ScanlationGroupName.AsuraScans)
        {
            scrapedSeries = await scraper.ScrapeSeriesAsync(seriesUrl);
            var normalizedTitle = NormalizeWhitespaces(scrapedSeries.Title);
            savedSeries = await seriesRepository.GetSeriesByTitleAsync(group.Id, normalizedTitle);
        }

        return (savedSeries, scrapedSeries);
    }

    private async Task UpdateSeriesFromGroupAsync(ScanlationGroupDto group, DateTimeOffset updateDate)
    {
        var seriesRepository = _seriesRepositoryFactory.CreateRepository();
        var urlManager = _urlManagerFactory.CreateUrlManager(
            group.Name,
            group.BaseWebsiteUrl,
            group.BaseCoverUrl);
        var scraper = _scraperFactory.CreateScraper(group.Name, urlManager.LatestUpdatesUrl);
        var idChangeIsLogged = false;

        await foreach (var seriesUpdate in scraper.ScrapeLatestUpdatesAsync())
        {
            var seriesExternalId = urlManager.ExtractSeriesId(seriesUpdate.SeriesUrl);
            IScrapedSeries? scrapedSeries = null;

            try
            {
                (var savedSeries, scrapedSeries) = await GetSeriesAsync(
                    seriesRepository,
                    scraper,
                    group,
                    seriesExternalId,
                    seriesUpdate.SeriesUrl);

                LogExternalIdChange(savedSeries, seriesExternalId, ref idChangeIsLogged);

                if (savedSeries is null)
                {
                    scrapedSeries ??= await scraper.ScrapeSeriesAsync(seriesUpdate.SeriesUrl);
                    await AddSeriesAsync(
                        seriesRepository,
                        scrapedSeries,
                        urlManager,
                        group,
                        seriesExternalId,
                        updateDate);
                }
                else
                {
                    var latestChapterExternalId = urlManager.ExtractChapterId(
                        seriesUpdate.LatestChapterUrl);
                    var latestSavedChapter = await seriesRepository.GetLatestChapterAsync(
                        savedSeries.Id);

                    if (latestChapterExternalId == latestSavedChapter.ExternalId
                        && seriesExternalId == savedSeries.ExternalId)
                    {
                        _logger.LogInformation(
                            "Latest chapter for series with Id {SeriesId} and external Id " +
                            "{SeriesExternalId} is already saved. Remaining series are up to date",
                            savedSeries.Id,
                            savedSeries.ExternalId);

                        break;
                    }

                    scrapedSeries ??= await scraper.ScrapeSeriesAsync(seriesUpdate.SeriesUrl);
                    await UpdateSingleSeriesAsync(
                        seriesRepository,
                        scrapedSeries,
                        urlManager,
                        savedSeries,
                        seriesExternalId,
                        latestSavedChapter,
                        updateDate,
                        group.Name);
                }
            }
            finally
            {
                if (scrapedSeries is not null)
                {
                    await scrapedSeries.DisposeAsync();
                }
            }
        }

        await seriesRepository.SaveChangesAsync();
    }

    private void LogExternalIdChange(
        SeriesDto? savedSeries,
        string seriesExternalId,
        ref bool idChangeIsLogged)
    {
        if (savedSeries is not null && seriesExternalId != savedSeries.ExternalId
            && !idChangeIsLogged)
        {
            _logger.LogWarning("Detected changes in external Ids");

            idChangeIsLogged = true;
        }
    }

    private async Task AddSeriesAsync(
        ISeriesRepository seriesRepository,
        IScrapedSeries scrapedSeries,
        IUrlManager urlManager,
        ScanlationGroupDto group,
        string seriesExternalId,
        DateTimeOffset updateDate)
    {
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
            seriesRepository,
            scrapedSeries,
            urlManager,
            series,
            updateDate,
            group.Name);
    }

    private async Task UpdateSingleSeriesAsync(
        ISeriesRepository seriesRepository,
        IScrapedSeries scrapedSeries,
        IUrlManager urlManager,
        SeriesDto savedSeries,
        string seriesExternalId,
        ChapterDto latestSavedChapter,
        DateTimeOffset updateDate,
        ScanlationGroupName groupName)
    {
        if (seriesExternalId != savedSeries.ExternalId)
        {
            savedSeries = savedSeries with { ExternalId = seriesExternalId };
            seriesRepository.UpdateSeries(savedSeries);

            _logger.LogInformation(
                "Updated external Id of series with Id {SeriesId} to {SeriesExternalId}",
                savedSeries.Id,
                savedSeries.ExternalId);
        }

        var relativeCoverUrl = urlManager.ExtractRelativeCoverUrl(scrapedSeries.CoverUrl);

        if (relativeCoverUrl != savedSeries.RelativeCoverUrl)
        {
            savedSeries = savedSeries with { RelativeCoverUrl = relativeCoverUrl };
            seriesRepository.UpdateSeries(savedSeries);

            _logger.LogInformation(
                "Updated cover URL for series with Id {SeriesId} and external Id {SeriesExternalId}",
                savedSeries.Id,
                savedSeries.ExternalId);
        }

        await AddChaptersAsync(
            seriesRepository,
            scrapedSeries,
            urlManager,
            savedSeries,
            updateDate,
            groupName,
            latestSavedChapter);
    }

    private async Task AddChaptersAsync(
        ISeriesRepository seriesRepository,
        IScrapedSeries scrapedSeries,
        IUrlManager urlManager,
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
