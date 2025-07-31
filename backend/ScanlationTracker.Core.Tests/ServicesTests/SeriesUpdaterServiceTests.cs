using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ScanlationTracker.Core.Metrics;
using ScanlationTracker.Core.Models;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.Scrapers.Contracts;
using ScanlationTracker.Core.Services;
using ScanlationTracker.Core.UrlManagers;
using System.Diagnostics.Metrics;
using Xunit;

namespace ScanlationTracker.Core.Tests.ServicesTests;

public class SeriesUpdaterServiceTests
{
    private readonly SeriesUpdaterService _seriesUpdaterService;

    private readonly ISeriesRepositoryFactory _seriesRepositoryFactory =
        Substitute.For<ISeriesRepositoryFactory>();

    private readonly IUrlManagerFactory _urlManagerFactory =
        Substitute.For<IUrlManagerFactory>();

    private readonly IScanlationScraperFactory _scraperFactory =
        Substitute.For<IScanlationScraperFactory>();

    public SeriesUpdaterServiceTests()
    {
        var meterFactory = Substitute.For<IMeterFactory>();
        meterFactory.Create(Arg.Any<MeterOptions>()).Returns(new Meter("Test Meter"));

        _seriesUpdaterService = new SeriesUpdaterService(
            _seriesRepositoryFactory,
            _urlManagerFactory,
            _scraperFactory,
            new NullLogger<SeriesUpdaterService>(),
            new SeriesUpdaterMetrics(meterFactory));
    }

    [Fact]
    public async Task UpdateSeriesAsync_AddsSeries()
    {
        // Arrange
        var latestUpdatesUrl = "https://asu.ra";
        var group = new ScanlationGroup()
        {
            Id = Guid.Parse("0197c07b-bbbb-777a-a143-71443604c4e6"),
            Name = ScanlationGroupName.AsuraScans,
            PublicName = ScanlationGroupName.AsuraScans.ToString(),
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var scrapedSeriesData = new
        {
            Url = "https://asu.ra/series/series-1",
            Title = "Series 1",
            CoverUrl = "https://gg.asu.ra/series-1.webp",
            Chapters = new ScrapedChapter[]
            {
                new()
                {
                    Url = "https://asu.ra/series/series-1/chapter/2",
                    Title = "Chapter 2",
                },
                new()
                {
                    Url = "https://asu.ra/series/series-1/chapter/1",
                    Title = "Chapter 1",
                },
            },
        };

        var savedSeriesId = Guid.Empty;

        var seriesRepository = Substitute.For<ISeriesRepository>();
        var urlManager = Substitute.For<IUrlManager>();
        var scrapedSeries = Substitute.For<IScrapedSeries>();
        var scraper = Substitute.For<IScanlationScraper>();

        seriesRepository
            .GetAllGroupsAsync()
            .Returns([group]);
        seriesRepository
            .When(repository => repository.AddSeries(
                Arg.Is<Series>(series => series.ExternalId == "series-1")))
            .Do(callInfo => savedSeriesId = callInfo.Arg<Series>().Id);

        _seriesRepositoryFactory
            .CreateRepository()
            .Returns(seriesRepository);

        urlManager
            .LatestUpdatesUrl
            .Returns(latestUpdatesUrl);
        urlManager
            .ExtractSeriesId(scrapedSeriesData.Url)
            .Returns("series-1");
        urlManager
            .ExtractRelativeCoverUrl(scrapedSeriesData.CoverUrl)
            .Returns("/series-1.webp");
        urlManager
            .ExtractChapterId(scrapedSeriesData.Chapters[0].Url)
            .Returns("2");
        urlManager
            .ExtractChapterId(scrapedSeriesData.Chapters[1].Url)
            .Returns("1");

        _urlManagerFactory
            .CreateUrlManager(group.Name, group.BaseWebsiteUrl, group.BaseCoverUrl)
            .Returns(urlManager);

        scrapedSeries
            .Title
            .Returns(scrapedSeriesData.Title);
        scrapedSeries
            .CoverUrl
            .Returns(scrapedSeriesData.CoverUrl);
        scrapedSeries
            .LatestChaptersAsync
            .Returns(ToAsync(scrapedSeriesData.Chapters));

        scraper
            .ScrapeUrlsOfLatestUpdatedSeriesAsync()
            .Returns(ToAsync([scrapedSeriesData.Url]));
        scraper
            .ScrapeSeriesAsync(scrapedSeriesData.Url)
            .Returns(scrapedSeries);

        _scraperFactory
            .CreateScraper(group.Name, latestUpdatesUrl)
            .Returns(scraper);

        // Act
        await _seriesUpdaterService.UpdateSeriesAsync();

        // Assert
        seriesRepository
            .Received()
            .AddSeries(Arg.Is<Series>(series =>
                series.Id != Guid.Empty
                && series.ScanlationGroupId == Guid.Parse("0197c07b-bbbb-777a-a143-71443604c4e6")
                && series.ExternalId == "series-1"
                && series.Title == "Series 1"
                && series.RelativeCoverUrl == "/series-1.webp"));
        seriesRepository
            .Received()
            .AddChapter(Arg.Is<Chapter>(chapter =>
                chapter.Id != Guid.Empty
                && chapter.SeriesId == savedSeriesId
                && chapter.ExternalId == "2"
                && chapter.Title == "Chapter 2"
                && chapter.Number == 2
                && chapter.AddedAt != default));
        seriesRepository
            .Received()
            .AddChapter(Arg.Is<Chapter>(chapter =>
                chapter.Id != Guid.Empty
                && chapter.SeriesId == savedSeriesId
                && chapter.ExternalId == "1"
                && chapter.Title == "Chapter 1"
                && chapter.Number == 1
                && chapter.AddedAt != default));
        await seriesRepository
            .Received()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateSeriesAsync_NormalizesTitles()
    {
        // Arrange
        var latestUpdatesUrl = "https://asu.ra";
        var group = new ScanlationGroup()
        {
            Id = Guid.Parse("0197c07b-bbbb-777a-a143-71443604c4e6"),
            Name = ScanlationGroupName.AsuraScans,
            PublicName = ScanlationGroupName.AsuraScans.ToString(),
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var scrapedSeriesData = new
        {
            Url = "https://asu.ra/series/series-1",
            Title = "Series \t\t\t 1",
            Chapters = new ScrapedChapter[]
            {
                new()
                {
                    Url = "https://asu.ra/series/series-1/chapter/2",
                    Title = "Chapter\t2",
                },
                new()
                {
                    Url = "https://asu.ra/series/series-1/chapter/1",
                    Title = " Chapter 1 ",
                },
            },
        };

        var seriesRepository = Substitute.For<ISeriesRepository>();
        var urlManager = Substitute.For<IUrlManager>();
        var scrapedSeries = Substitute.For<IScrapedSeries>();
        var scraper = Substitute.For<IScanlationScraper>();

        seriesRepository
            .GetAllGroupsAsync()
            .Returns([group]);

        _seriesRepositoryFactory
            .CreateRepository()
            .Returns(seriesRepository);

        urlManager
            .LatestUpdatesUrl
            .Returns(latestUpdatesUrl);

        _urlManagerFactory
            .CreateUrlManager(group.Name, group.BaseWebsiteUrl, group.BaseCoverUrl)
            .Returns(urlManager);

        scrapedSeries
            .Title
            .Returns(scrapedSeriesData.Title);
        scrapedSeries
            .LatestChaptersAsync
            .Returns(ToAsync(scrapedSeriesData.Chapters));

        scraper
            .ScrapeUrlsOfLatestUpdatedSeriesAsync()
            .Returns(ToAsync([scrapedSeriesData.Url]));
        scraper
            .ScrapeSeriesAsync(scrapedSeriesData.Url)
            .Returns(scrapedSeries);

        _scraperFactory
            .CreateScraper(group.Name, latestUpdatesUrl)
            .Returns(scraper);

        // Act
        await _seriesUpdaterService.UpdateSeriesAsync();

        // Assert
        seriesRepository
            .Received()
            .AddSeries(Arg.Is<Series>(series => series.Title == "Series 1"));
        seriesRepository
            .Received()
            .AddChapter(Arg.Is<Chapter>(chapter => chapter.Title == "Chapter 2"));
        seriesRepository
            .Received()
            .AddChapter(Arg.Is<Chapter>(chapter => chapter.Title == "Chapter 1"));
        await seriesRepository
            .Received()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateSeriesAsync_UpdatesSeries()
    {
        // Arrange
        var latestUpdatesUrl = "https://asu.ra";
        var group = new ScanlationGroup()
        {
            Id = Guid.Parse("0197c07b-bbbb-777a-a143-71443604c4e6"),
            Name = ScanlationGroupName.AsuraScans,
            PublicName = ScanlationGroupName.AsuraScans.ToString(),
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var savedSeries = new Series
        {
            Id = Guid.Parse("0197cce4-ac80-7c2d-9426-2d69ba7de348"),
            ScanlationGroupId = group.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        var latestSavedChapter = new Chapter
        {
            Id = Guid.Parse("0197d19d-04e6-7867-9236-bd33caabfb4b"),
            SeriesId = savedSeries.Id,
            ExternalId = "1",
            Title = "Chapter 1",
            Number = 1,
            AddedAt = new DateTimeOffset(2025, 7, 3, 0, 0, 0, default),
        };

        var scrapedSeriesData = new
        {
            Url = "https://asu.ra/series/series-1",
            CoverUrl = "https://gg.asu.ra/series-1-updated.webp",
            Chapters = new ScrapedChapter[]
            {
                new()
                {
                    Url = "https://asu.ra/series/series-1/chapter/2",
                    Title = "Chapter 2",
                },
                new()
                {
                    Url = "https://asu.ra/series/series-1/chapter/1",
                    Title = "Chapter 1",
                },
            },
        };

        var seriesRepository = Substitute.For<ISeriesRepository>();
        var urlManager = Substitute.For<IUrlManager>();
        var scrapedSeries = Substitute.For<IScrapedSeries>();
        var scraper = Substitute.For<IScanlationScraper>();

        seriesRepository
            .GetAllGroupsAsync()
            .Returns([group]);
        seriesRepository
            .GetSeriesByExternalIdAsync(group.Id, "series-1")
            .Returns(savedSeries);
        seriesRepository
            .GetLatestChapterAsync(savedSeries.Id)
            .Returns(latestSavedChapter);

        _seriesRepositoryFactory
            .CreateRepository()
            .Returns(seriesRepository);

        urlManager
            .LatestUpdatesUrl
            .Returns(latestUpdatesUrl);
        urlManager
            .ExtractSeriesId(scrapedSeriesData.Url)
            .Returns("series-1");
        urlManager
            .ExtractRelativeCoverUrl(scrapedSeriesData.CoverUrl)
            .Returns("/series-1-updated.webp");
        urlManager
            .ExtractChapterId(scrapedSeriesData.Chapters[0].Url)
            .Returns("2");
        urlManager
            .ExtractChapterId(scrapedSeriesData.Chapters[1].Url)
            .Returns("1");

        _urlManagerFactory
            .CreateUrlManager(group.Name, group.BaseWebsiteUrl, group.BaseCoverUrl)
            .Returns(urlManager);

        scrapedSeries
            .CoverUrl
            .Returns(scrapedSeriesData.CoverUrl);
        scrapedSeries
            .LatestChaptersAsync
            .Returns(ToAsync(scrapedSeriesData.Chapters));

        scraper
            .ScrapeUrlsOfLatestUpdatedSeriesAsync()
            .Returns(ToAsync([scrapedSeriesData.Url]));
        scraper
            .ScrapeSeriesAsync(scrapedSeriesData.Url)
            .Returns(scrapedSeries);

        _scraperFactory
            .CreateScraper(group.Name, latestUpdatesUrl)
            .Returns(scraper);

        // Act
        await _seriesUpdaterService.UpdateSeriesAsync();

        // Assert
        seriesRepository
            .Received()
            .UpdateSeries(Arg.Is<Series>(series =>
                series.Id == Guid.Parse("0197cce4-ac80-7c2d-9426-2d69ba7de348")
                && series.ScanlationGroupId == Guid.Parse("0197c07b-bbbb-777a-a143-71443604c4e6")
                && series.ExternalId == "series-1"
                && series.Title == "Series 1"
                && series.RelativeCoverUrl == "/series-1-updated.webp"));
        seriesRepository
            .Received()
            .AddChapter(Arg.Is<Chapter>(chapter =>
                chapter.Id != Guid.Empty
                && chapter.SeriesId == Guid.Parse("0197cce4-ac80-7c2d-9426-2d69ba7de348")
                && chapter.ExternalId == "2"
                && chapter.Title == "Chapter 2"
                && chapter.Number == 2
                && chapter.AddedAt != default));
        seriesRepository
            .DidNotReceive()
            .AddChapter(Arg.Is<Chapter>(chapter => chapter.ExternalId == "1"));
        await seriesRepository
            .Received()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateSeriesAsync_UpdatesExternalId()
    {
        // Arrange
        var latestUpdatesUrl = "https://asu.ra";
        var group = new ScanlationGroup()
        {
            Id = Guid.Parse("0197c07b-bbbb-777a-a143-71443604c4e6"),
            Name = ScanlationGroupName.AsuraScans,
            PublicName = ScanlationGroupName.AsuraScans.ToString(),
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var savedSeries = new Series
        {
            Id = Guid.Parse("0197cce4-ac80-7c2d-9426-2d69ba7de348"),
            ScanlationGroupId = group.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        var scrapedSeriesData = new
        {
            Url = "https://asu.ra/series/series-1-updated",
            Title = "Series \t\t\t 1",
        };

        var seriesRepository = Substitute.For<ISeriesRepository>();
        var urlManager = Substitute.For<IUrlManager>();
        var scrapedSeries = Substitute.For<IScrapedSeries>();
        var scraper = Substitute.For<IScanlationScraper>();

        seriesRepository
            .GetAllGroupsAsync()
            .Returns([group]);
        seriesRepository
            .GetSeriesByTitleAsync(group.Id, savedSeries.Title)
            .Returns([savedSeries]);

        _seriesRepositoryFactory
            .CreateRepository()
            .Returns(seriesRepository);

        urlManager
            .LatestUpdatesUrl
            .Returns(latestUpdatesUrl);
        urlManager
            .ExtractSeriesId(scrapedSeriesData.Url)
            .Returns("series-1-updated");

        _urlManagerFactory
            .CreateUrlManager(group.Name, group.BaseWebsiteUrl, group.BaseCoverUrl)
            .Returns(urlManager);

        scrapedSeries
            .Title
            .Returns(scrapedSeriesData.Title);

        scraper
            .ScrapeUrlsOfLatestUpdatedSeriesAsync()
            .Returns(ToAsync([scrapedSeriesData.Url]));
        scraper
            .ScrapeSeriesAsync(scrapedSeriesData.Url)
            .Returns(scrapedSeries);

        _scraperFactory
            .CreateScraper(group.Name, latestUpdatesUrl)
            .Returns(scraper);

        // Act
        await _seriesUpdaterService.UpdateSeriesAsync();

        // Assert
        seriesRepository
            .Received()
            .UpdateSeries(Arg.Is<Series>(series =>
                series.Id == Guid.Parse("0197cce4-ac80-7c2d-9426-2d69ba7de348")
                && series.ExternalId == "series-1-updated"));
        await seriesRepository
            .Received()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateSeriesAsync_Aborts_WhenMultipleSeriesFoundByTitle()
    {
        // Arrange
        var latestUpdatesUrl = "https://asu.ra";
        var group = new ScanlationGroup()
        {
            Id = Guid.Parse("0197c07b-bbbb-777a-a143-71443604c4e6"),
            Name = ScanlationGroupName.AsuraScans,
            PublicName = ScanlationGroupName.AsuraScans.ToString(),
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var savedSeries = new Series[]
        {
            new()
            {
                Id = Guid.Parse("0197d664-0053-7cfb-ba4a-2f4f88eb6cad"),
                ScanlationGroupId = group.Id,
                ExternalId = "series-2",
                Title = "Series",
                RelativeCoverUrl = "/series-2.webp",
            },
            new()
            {
                Id = Guid.Parse("0197cce4-ac80-7c2d-9426-2d69ba7de348"),
                ScanlationGroupId = group.Id,
                ExternalId = "series-1",
                Title = "Series",
                RelativeCoverUrl = "/series-1.webp",
            },
        };

        var scrapedSeriesData = new
        {
            Url = "https://asu.ra/series/series-1-updated",
            Title = "Series",
        };

        var seriesRepository = Substitute.For<ISeriesRepository>();
        var urlManager = Substitute.For<IUrlManager>();
        var scrapedSeries = Substitute.For<IScrapedSeries>();
        var scraper = Substitute.For<IScanlationScraper>();

        seriesRepository
            .GetAllGroupsAsync()
            .Returns([group]);
        seriesRepository
            .GetSeriesByTitleAsync(group.Id, savedSeries[0].Title)
            .Returns(savedSeries);

        _seriesRepositoryFactory
            .CreateRepository()
            .Returns(seriesRepository);

        urlManager
            .LatestUpdatesUrl
            .Returns(latestUpdatesUrl);

        _urlManagerFactory
            .CreateUrlManager(group.Name, group.BaseWebsiteUrl, group.BaseCoverUrl)
            .Returns(urlManager);

        scrapedSeries
            .Title
            .Returns(scrapedSeriesData.Title);

        scraper
            .ScrapeUrlsOfLatestUpdatedSeriesAsync()
            .Returns(ToAsync([scrapedSeriesData.Url]));
        scraper
            .ScrapeSeriesAsync(scrapedSeriesData.Url)
            .Returns(scrapedSeries);

        _scraperFactory
            .CreateScraper(group.Name, latestUpdatesUrl)
            .Returns(scraper);

        // Act
        await _seriesUpdaterService.UpdateSeriesAsync();

        // Assert
        await seriesRepository
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateSeriesAsync_Stops_WhenEverythingIsUpdated()
    {
        // Arrange
        var latestUpdatesUrl = "https://asu.ra";
        var group = new ScanlationGroup()
        {
            Id = Guid.Parse("0197c07b-bbbb-777a-a143-71443604c4e6"),
            Name = ScanlationGroupName.AsuraScans,
            PublicName = ScanlationGroupName.AsuraScans.ToString(),
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var savedSeries = new Series[]
        {
            new()
            {
                Id = Guid.Parse("0197d664-0053-79ed-80df-7f37057f890c"),
                ScanlationGroupId = group.Id,
                ExternalId = "series-3",
                Title = "Series 3",
                RelativeCoverUrl = "/series-3.webp",
            },
            new()
            {
                Id = Guid.Parse("0197d664-0053-7cfb-ba4a-2f4f88eb6cad"),
                ScanlationGroupId = group.Id,
                ExternalId = "series-2",
                Title = "Series 2",
                RelativeCoverUrl = "/series-2.webp",
            },
        };

        var scrapedSeriesData = new[]
        {
            new
            {
                Url = "https://asu.ra/series/series-3",
                CoverUrl = "https://gg.asu.ra/series-3-updated.webp",
            },
            new
            {
                Url = "https://asu.ra/series/series-2",
                CoverUrl = "https://gg.asu.ra/series-2.webp",
            },
            new
            {
                Url = "https://asu.ra/series/series-1",
                CoverUrl = "https://gg.asu.ra/series-1.webp",
            },
        };

        var seriesRepository = Substitute.For<ISeriesRepository>();
        var urlManager = Substitute.For<IUrlManager>();
        var scraper = Substitute.For<IScanlationScraper>();

        seriesRepository
            .GetAllGroupsAsync()
            .Returns([group]);
        seriesRepository
            .GetSeriesByExternalIdAsync(group.Id, "series-3")
            .Returns(savedSeries[0]);
        seriesRepository
            .GetSeriesByExternalIdAsync(group.Id, "series-2")
            .Returns(savedSeries[1]);

        _seriesRepositoryFactory
            .CreateRepository()
            .Returns(seriesRepository);

        urlManager
            .LatestUpdatesUrl
            .Returns(latestUpdatesUrl);
        urlManager
            .ExtractSeriesId(scrapedSeriesData[0].Url)
            .Returns("series-3");
        urlManager
            .ExtractSeriesId(scrapedSeriesData[1].Url)
            .Returns("series-2");
        urlManager
            .ExtractRelativeCoverUrl(scrapedSeriesData[0].CoverUrl)
            .Returns("/series-3-updated.webp");
        urlManager
            .ExtractRelativeCoverUrl(scrapedSeriesData[1].CoverUrl)
            .Returns("/series-2.webp");

        _urlManagerFactory
            .CreateUrlManager(group.Name, group.BaseWebsiteUrl, group.BaseCoverUrl)
            .Returns(urlManager);

        for (var index = 0; index < scrapedSeriesData.Length; index++)
        {
            var scrapedSeries = Substitute.For<IScrapedSeries>();

            scrapedSeries
                .CoverUrl
                .Returns(scrapedSeriesData[index].CoverUrl);

            scraper
                .ScrapeSeriesAsync(scrapedSeriesData[index].Url)
                .Returns(scrapedSeries);
        }

        scraper
            .ScrapeUrlsOfLatestUpdatedSeriesAsync()
            .Returns(ToAsync(scrapedSeriesData.Select(series => series.Url)));

        _scraperFactory
            .CreateScraper(group.Name, latestUpdatesUrl)
            .Returns(scraper);

        // Act
        await _seriesUpdaterService.UpdateSeriesAsync();

        // Assert
        seriesRepository
            .Received()
            .UpdateSeries(Arg.Is<Series>(series =>
                series.Id == Guid.Parse("0197d664-0053-79ed-80df-7f37057f890c")));

        await scraper
            .Received()
            .ScrapeSeriesAsync("https://asu.ra/series/series-2");
        seriesRepository
            .DidNotReceive()
            .UpdateSeries(Arg.Is<Series>(series =>
                series.Id == Guid.Parse("0197d664-0053-7cfb-ba4a-2f4f88eb6cad")));

        await scraper
            .DidNotReceive()
            .ScrapeSeriesAsync("https://asu.ra/series/series-1");

        await seriesRepository
            .Received()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateSeriesAsync_CatchesExceptions()
    {
        // Arrange
        var latestUpdatesUrl = "https://asu.ra";
        var group = new ScanlationGroup()
        {
            Id = Guid.Parse("0197c07b-bbbb-777a-a143-71443604c4e6"),
            Name = ScanlationGroupName.AsuraScans,
            PublicName = ScanlationGroupName.AsuraScans.ToString(),
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var seriesRepository = Substitute.For<ISeriesRepository>();
        var urlManager = Substitute.For<IUrlManager>();
        var scraper = Substitute.For<IScanlationScraper>();

        seriesRepository
            .GetAllGroupsAsync()
            .Returns([group]);

        _seriesRepositoryFactory
            .CreateRepository()
            .Returns(seriesRepository);

        urlManager
            .LatestUpdatesUrl
            .Returns(latestUpdatesUrl);

        _urlManagerFactory
            .CreateUrlManager(group.Name, group.BaseWebsiteUrl, group.BaseCoverUrl)
            .Returns(urlManager);

        scraper
            .ScrapeUrlsOfLatestUpdatedSeriesAsync()
            .Throws<Exception>();

        _scraperFactory
            .CreateScraper(group.Name, latestUpdatesUrl)
            .Returns(scraper);

        // Act
        var exception = await Record.ExceptionAsync(_seriesUpdaterService.UpdateSeriesAsync);

        // Assert
        Assert.Null(exception);
    }

    private static async IAsyncEnumerable<TSource> ToAsync<TSource>(IEnumerable<TSource> source)
    {
        foreach (var element in source)
        {
            yield return element;
        }

        await Task.CompletedTask;
    }
}
