using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.Scrapers.Dtos;
using ScanlationTracker.Infrastructure.UrlHelpers;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal class RizzFablesAsScraper : PaginatedAsScraper, IScanlationScraper
{
    private const StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;

    public RizzFablesAsScraper(
        IHttpClientFactory httpClientFactory,
        RizzFablesUrlHelper urlHelper,
        ILogger<RizzFablesAsScraper> logger)
        : base(httpClientFactory, urlHelper.LatestUpdatesUrl, logger)
        => HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Chrome");

    public async Task<ScrapedSeries> ScrapeSeriesAsync(string seriesUrl)
    {
        Logger.LogInformation("Scraping series at {SeriesUrl}", seriesUrl);

        var document = await BrowsingContext.OpenAsync(seriesUrl);

        var title = document
            .All
            .First(element => element.TextContent.Trim().StartsWith("Synopsis", _stringComparison))
            .ParentElement!
            .FindDescendant<IHtmlHeadingElement>()!
            .TextContent;

        var coverUrl = document
            .All
            .First(element => element.TextContent.Trim().Equals("Bookmark", _stringComparison))
            .ParentElement!
            .FindDescendant<IHtmlImageElement>()!
            .Source!;

        var chaptersAnchors = document
            .GetElementById("chapterlist")!
            .Descendants<IHtmlAnchorElement>();

        var latestChapters = chaptersAnchors.Select(anchor =>
            new ScrapedChapter { Url = anchor.Href, Title = anchor.Children[0].TextContent });

        return new ScrapedSeries
        {
            Title = title,
            CoverUrl = coverUrl,
            LatestChapters = latestChapters,
        };
    }

    protected override IEnumerable<ScrapedSeriesUpdate> ScrapeSeriesUpdates(IDocument document)
    {
        var updatesSection = document
            .All
            .First(element => element.TextContent.Equals("Latest Update", _stringComparison))
            .ParentElement!
            .NextElementSibling!;

        foreach (var updateElement in updatesSection.Children)
        {
            var anchors = updateElement.Descendants<IHtmlAnchorElement>().Take(1..3).ToArray();

            yield return new ScrapedSeriesUpdate
            {
                SeriesUrl = anchors[0].Href,
                LatestChapterUrl = anchors[1].Href,
            };
        }
    }

    protected override string? ScrapeNextPageUrl(IDocument document)
    {
        var nextPageAnchor = (IHtmlAnchorElement?)document
            .Links
            .FirstOrDefault(anchor => anchor.TextContent.TrimEnd().Equals("Next", _stringComparison));

        return nextPageAnchor?.Href;
    }
}
