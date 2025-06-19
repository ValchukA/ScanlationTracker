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

    private static async Task<SeriesDto?> GetSavedSeriesAsync(
        ISeriesRepository seriesRepository,
        IScrapedSeries scrapedSeries,
        ScanlationGroupDto group,
        string seriesExternalId)
    {
        var savedSeries = await seriesRepository.GetSeriesByExternalIdAsync(
            group.Id,
            seriesExternalId);

        if (savedSeries is null && group.Name == ScanlationGroupName.AsuraScans)
        {
            var normalizedTitle = NormalizeWhitespaces(scrapedSeries.Title);
            savedSeries = await seriesRepository.GetSeriesByTitleAsync(group.Id, normalizedTitle);
        }

        return savedSeries;
    }

    private void LogExternalIdChange(
        SeriesDto? savedSeries,
        string seriesExternalId,
        ref bool idChangeLogged)
    {
        if (savedSeries is not null && seriesExternalId != savedSeries.ExternalId
            && !idChangeLogged)
        {
            _logger.LogWarning("Detected changes in external Ids");

            idChangeLogged = true;
        }
    }

    private async Task UpdateSeriesFromGroupAsync(ScanlationGroupDto group, DateTimeOffset updateDate)
    {
        var seriesRepository = _seriesRepositoryFactory.CreateRepository();
        var urlManager = _urlManagerFactory.CreateUrlManager(
            group.Name,
            group.BaseWebsiteUrl,
            group.BaseCoverUrl);
        var scraper = _scraperFactory.CreateScraper(group.Name, urlManager.LatestUpdatesUrl);
        var idChangeLogged = false;

        await foreach (var seriesUrl in scraper.ScrapeUrlsOfLatestUpdatedSeriesAsync())
        {
            var seriesExternalId = urlManager.ExtractSeriesId(seriesUrl);
            await using var scrapedSeries = await scraper.ScrapeSeriesAsync(seriesUrl);
            var savedSeries = await GetSavedSeriesAsync(
                seriesRepository,
                scrapedSeries,
                group,
                seriesExternalId);

            LogExternalIdChange(savedSeries, seriesExternalId, ref idChangeLogged);

            if (savedSeries is null)
            {
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
                var updatePerformed = await UpdateSeriesIfOutdatedAsync(
                    seriesRepository,
                    scrapedSeries,
                    urlManager,
                    savedSeries,
                    seriesExternalId,
                    updateDate,
                    group.Name);

                if (!updatePerformed)
                {
                    _logger.LogInformation("Remaining series are up to date");

                    break;
                }
            }
        }

        await seriesRepository.SaveChangesAsync();
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

    private async Task<bool> UpdateSeriesIfOutdatedAsync(
        ISeriesRepository seriesRepository,
        IScrapedSeries scrapedSeries,
        IUrlManager urlManager,
        SeriesDto savedSeries,
        string seriesExternalId,
        DateTimeOffset updateDate,
        ScanlationGroupName groupName)
    {
        var updatePerformed = false;

        if (seriesExternalId != savedSeries.ExternalId)
        {
            savedSeries = savedSeries with { ExternalId = seriesExternalId };
            seriesRepository.UpdateSeries(savedSeries);
            updatePerformed = true;

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
            updatePerformed = true;

            _logger.LogInformation(
                "Updated cover URL for series with Id {SeriesId} and external Id {SeriesExternalId}",
                savedSeries.Id,
                savedSeries.ExternalId);
        }

        var latestSavedChapter = await seriesRepository.GetLatestChapterAsync(savedSeries.Id);
        updatePerformed |= await AddChaptersAsync(
            seriesRepository,
            scrapedSeries,
            urlManager,
            savedSeries,
            updateDate,
            groupName,
            latestSavedChapter);

        return updatePerformed;
    }

    private async Task<bool> AddChaptersAsync(
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

        return chaptersToSave.Count > 0;
    }
}
