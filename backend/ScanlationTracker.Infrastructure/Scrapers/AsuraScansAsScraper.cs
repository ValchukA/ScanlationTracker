using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.Scrapers.Dtos;
using ScanlationTracker.Infrastructure.UrlHelpers;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal class AsuraScansAsScraper : PaginatedAsScraper, IScanlationScraper
{
    private const StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;

    public AsuraScansAsScraper(HttpClient httpClient, AsuraScansUrlHelper urlHelper, ILogger<AsuraScansAsScraper> logger)
        : base(httpClient, urlHelper.LatestUpdatesUrl, logger)
    {
    }

    public async Task<ScrapedSeries> ScrapeSeriesAsync(string seriesUrl)
    {
        Logger.LogInformation("Scraping series at {SeriesUrl}", seriesUrl);

        var document = await BrowsingContext.OpenAsync(seriesUrl);

        var title = document
            .All
            .First(element => element.TextContent.StartsWith("Synopsis", _stringComparison))
            .ParentElement!
            .FirstElementChild!
            .TextContent;

        var coverUrl = ((IHtmlImageElement)document
            .All
            .First(element => element.TextContent.Equals("Bookmark", _stringComparison))
            .ParentElement!
            .PreviousElementSibling!)
            .Source!;

        var chaptersAnchors = document
            .All
            .OfType<IHtmlInputElement>()
            .First(input => input.Placeholder!.StartsWith("Search Chapter", _stringComparison))
            .ParentElement!
            .NextElementSibling!
            .Descendants<IHtmlAnchorElement>();

        var latestChapters = chaptersAnchors.Select(anchor =>
        {
            var titleParts = anchor.Children[1]
                .Descendants<IText>().Select(node => node.TextContent.TrimEnd()).ToArray();

            return new ScrapedChapter { Url = anchor.Href, Title = string.Join(' ', titleParts) };
        });

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
            .First(element => element.TextContent.Equals("Latest Updates", _stringComparison))
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
        var nextPageAnchor = (IHtmlAnchorElement)document
            .Links
            .First(anchor => anchor.TextContent.Equals("Next", _stringComparison));

        return nextPageAnchor.Href.EndsWith('#') ? null : nextPageAnchor.Href;
    }
}
