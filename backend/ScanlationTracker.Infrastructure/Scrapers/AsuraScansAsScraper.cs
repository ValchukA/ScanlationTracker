using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io.Network;
using Microsoft.Extensions.Logging;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.Scrapers.Dtos;
using ScanlationTracker.Infrastructure.UrlHelpers;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal class AsuraScansAsScraper : IScanlationScraper
{
    private const StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;

    private readonly IBrowsingContext _browsingContext;
    private readonly string _latestUpdatesUrl;
    private readonly ILogger<AsuraScansAsScraper> _logger;

    public AsuraScansAsScraper(HttpClient httpClient, AsuraScansUrlHelper urlHelper, ILogger<AsuraScansAsScraper> logger)
    {
        var angleSharpConfig = Configuration.Default
            .WithRequester(new HttpClientRequester(httpClient)).WithDefaultLoader();

        _browsingContext = BrowsingContext.New(angleSharpConfig);
        _latestUpdatesUrl = urlHelper.LatestUpdatesUrl;
        _logger = logger;
    }

    public async IAsyncEnumerable<ScrapedSeriesUpdate> ScrapeLatestUpdatesAsync()
    {
        var scrapedSeriesUrls = new HashSet<string>();
        var pageUrl = _latestUpdatesUrl;

        while (true)
        {
            _logger.LogInformation("Scraping series updates at {PageUrl}", pageUrl);

            var document = await _browsingContext.OpenAsync(pageUrl);

            foreach (var seriesUpdate in ScrapeSeriesUpdates(document))
            {
                if (scrapedSeriesUrls.Add(seriesUpdate.SeriesUrl))
                {
                    yield return seriesUpdate;

                    continue;
                }

                _logger.LogInformation("Pages shifted between iterations");
            }

            pageUrl = ScrapeNextPageUrl(document);

            if (pageUrl is null)
            {
                _logger.LogInformation("There are no remaining pages to scrape");

                yield break;
            }
        }
    }

    public async Task<ScrapedSeries> ScrapeSeriesAsync(string seriesUrl)
    {
        _logger.LogInformation("Scraping series at {SeriesUrl}", seriesUrl);

        var document = await _browsingContext.OpenAsync(seriesUrl);

        var coverUrl = ((IHtmlImageElement)document
            .All
            .First(element => element.TextContent.Equals("Bookmark", _stringComparison))
            .ParentElement!
            .PreviousElementSibling!)
            .Source!;

        var title = document
            .All
            .First(element => element.TextContent.StartsWith("Synopsis", _stringComparison))
            .ParentElement!
            .FirstElementChild!
            .TextContent;

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

    private static IEnumerable<ScrapedSeriesUpdate> ScrapeSeriesUpdates(IDocument document)
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

    private static string? ScrapeNextPageUrl(IDocument document)
    {
        var nextPageAnchor = (IHtmlAnchorElement)document
            .Links
            .First(anchor => anchor.TextContent.Equals("Next", _stringComparison));

        return nextPageAnchor.Href.EndsWith('#') ? null : nextPageAnchor.Href;
    }
}
