using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.Scrapers.Dtos;
using ScanlationTracker.Infrastructure.Scrapers.BrowserContext;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal class AsuraScansPwScraper : IScanlationScraper
{
    private readonly IPwBrowserContextHolder _pwBrowserContextHolder;
    private readonly string _latestUpdatesUrl;
    private readonly ILogger _logger;

    public AsuraScansPwScraper(
        IPwBrowserContextHolder pwBrowserContextHolder,
        string latestUpdatesUrl,
        ILogger<AsuraScansPwScraper> logger)
    {
        _pwBrowserContextHolder = pwBrowserContextHolder;
        _latestUpdatesUrl = latestUpdatesUrl;
        _logger = logger;
    }

    public async IAsyncEnumerable<ScrapedSeriesUpdate> ScrapeLatestUpdatesAsync()
    {
        var context = await _pwBrowserContextHolder.GetContextAsync();
        var page = await context.NewPageAsync();

        try
        {
            var scrapedSeriesUrls = new HashSet<string>();

            await page.GotoAsync(_latestUpdatesUrl);

            while (true)
            {
                _logger.LogInformation("Scraping series updates at {PageUrl}", page.Url);

                var latestUpdates = ScrapeLatestUpdatesFromPageAsync(page, scrapedSeriesUrls);

                await foreach (var scrapedSeries in latestUpdates)
                {
                    yield return scrapedSeries;
                }

                var nextButtonLocator = page.Locator("a:has-text('Next')");
                var nextPageExists = await nextButtonLocator.GetAttributeAsync("href") != "#";

                if (!nextPageExists)
                {
                    _logger.LogInformation("There are no remaining pages to scrape");

                    yield break;
                }

                await nextButtonLocator.ClickAsync();
                await page.WaitForLoadStateAsync();
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public async Task<IScrapedSeries> ScrapeSeriesAsync(string seriesUrl)
    {
        _logger.LogInformation("Scraping series at {SeriesUrl}", seriesUrl);

        var context = await _pwBrowserContextHolder.GetContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync(seriesUrl);

        var title = (await page
            .Locator("div:has(~ h3:has-text('Synopsis')):nth-of-type(1)").TextContentAsync())!;
        var coverUrl = (await page
            .Locator("img:has(+ div:has-text('Bookmark'))").GetAttributeAsync("src"))!;

        return new PwScrapedSeries(page)
        {
            Title = title,
            CoverUrl = coverUrl,
            LatestChaptersAsync = ScrapeChaptersAsync(page),
        };
    }

    private async IAsyncEnumerable<ScrapedSeriesUpdate> ScrapeLatestUpdatesFromPageAsync(
        IPage page,
        HashSet<string> scrapedSeriesUrls)
    {
        var updateLocators = await page
            .Locator("div:has(> h3:has-text('Latest Updates')) + div > div").AllAsync();

        foreach (var updateLocator in updateLocators)
        {
            var seriesUrl = await updateLocator
                .Locator(":nth-match(a, 2)").EvaluateAsync<string>("a => a.href");

            if (scrapedSeriesUrls.Add(seriesUrl))
            {
                var latestChapterUrl = await updateLocator
                    .Locator(":nth-match(a, 3)").EvaluateAsync<string>("a => a.href");

                yield return new ScrapedSeriesUpdate
                {
                    SeriesUrl = seriesUrl,
                    LatestChapterUrl = latestChapterUrl,
                };

                continue;
            }

            _logger.LogInformation("Pages shifted between iterations");
        }
    }

    private async IAsyncEnumerable<ScrapedChapter> ScrapeChaptersAsync(IPage page)
    {
        var scrapedChapterUrls = new HashSet<string>();
        var chapterLocators = await page
            .Locator("div:has(> [placeholder^='Search Chapter' i]) + div > div > a").AllAsync();

        foreach (var chapterLocator in chapterLocators)
        {
            var url = await chapterLocator.EvaluateAsync<string>("a => a.href");

            if (scrapedChapterUrls.Add(url))
            {
                var title = await chapterLocator
                    .Locator("> h3:nth-of-type(1)").EvaluateAsync<string>("""
                        h3 => [...h3.childNodes]
                            .filter(node => [Node.TEXT_NODE, Node.ELEMENT_NODE].includes(node.nodeType)
                                && node.textContent)
                            .map(node => node.textContent.trimEnd())
                            .join(' ')
                    """);

                yield return new ScrapedChapter { Url = url, Title = title };

                continue;
            }

            _logger.LogInformation("Detected duplicated chapters in the list");
        }
    }
}
