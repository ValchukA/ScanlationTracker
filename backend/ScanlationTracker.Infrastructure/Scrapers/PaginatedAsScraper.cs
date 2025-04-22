using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io.Network;
using Microsoft.Extensions.Logging;
using ScanlationTracker.Core.Scrapers.Dtos;
using AsBrowsingContext = AngleSharp.BrowsingContext;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal abstract class PaginatedAsScraper
{
    private readonly string _latestUpdatesUrl;

    protected PaginatedAsScraper(IHttpClientFactory httpClientFactory, string latestUpdatesUrl, ILogger logger)
    {
        _latestUpdatesUrl = latestUpdatesUrl;
        Logger = logger;

        HttpClient = httpClientFactory.CreateClient(latestUpdatesUrl);
        var angleSharpConfig = Configuration.Default
            .WithRequester(new HttpClientRequester(HttpClient)).WithDefaultLoader();
        BrowsingContext = AsBrowsingContext.New(angleSharpConfig);
    }

    protected HttpClient HttpClient { get; }

    protected IBrowsingContext BrowsingContext { get; }

    protected ILogger Logger { get; }

    public async IAsyncEnumerable<ScrapedSeriesUpdate> ScrapeLatestUpdatesAsync()
    {
        var scrapedSeriesUrls = new HashSet<string>();
        var pageUrl = _latestUpdatesUrl;

        while (true)
        {
            Logger.LogInformation("Scraping series updates at {PageUrl}", pageUrl);

            var document = await BrowsingContext.OpenAsync(pageUrl);

            foreach (var seriesUpdate in ScrapeSeriesUpdates(document))
            {
                if (scrapedSeriesUrls.Add(seriesUpdate.SeriesUrl))
                {
                    yield return seriesUpdate;

                    continue;
                }

                Logger.LogInformation("Pages shifted between iterations");
            }

            pageUrl = ScrapeNextPageUrl(document);

            if (pageUrl is null)
            {
                Logger.LogInformation("There are no remaining pages to scrape");

                yield break;
            }
        }
    }

    protected abstract IEnumerable<ScrapedSeriesUpdate> ScrapeSeriesUpdates(IDocument document);

    protected abstract string? ScrapeNextPageUrl(IDocument document);
}
