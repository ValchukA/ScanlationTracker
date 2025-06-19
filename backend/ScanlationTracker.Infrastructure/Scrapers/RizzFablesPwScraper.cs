using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.Scrapers.Dtos;
using ScanlationTracker.Infrastructure.Scrapers.BrowserContext;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal class RizzFablesPwScraper : IScanlationScraper
{
    private readonly IPwBrowserContextHolder _pwBrowserContextHolder;
    private readonly string _latestUpdatesUrl;
    private readonly string _domain;
    private readonly ILogger _logger;

    public RizzFablesPwScraper(
        IPwBrowserContextHolder pwBrowserContextHolder,
        string latestUpdatesUrl,
        ILogger<RizzFablesPwScraper> logger)
    {
        _pwBrowserContextHolder = pwBrowserContextHolder;
        _latestUpdatesUrl = latestUpdatesUrl;
        _domain = $".{new Uri(latestUpdatesUrl).Host}";
        _logger = logger;
    }

    public async IAsyncEnumerable<string> ScrapeUrlsOfLatestUpdatedSeriesAsync()
    {
        var context = await _pwBrowserContextHolder.GetContextAsync();
        var page = await context.NewPageAsync();

        try
        {
            var scrapedSeriesUrls = new HashSet<string>();

            await context.ClearCookiesAsync(new() { Domain = _domain });
            await page.GotoAsync(_latestUpdatesUrl);

            while (true)
            {
                _logger.LogInformation("Scraping updated series at {PageUrl}", page.Url);

                var seriesUrls = ScrapeSeriesUrlsFromPageAsync(page, scrapedSeriesUrls);

                await foreach (var seriesUrl in seriesUrls)
                {
                    yield return seriesUrl;
                }

                var nextButtonLocator = page.Locator("a:has-text('Next')");
                var nextPageExists = await nextButtonLocator.CountAsync() == 1;

                if (!nextPageExists)
                {
                    _logger.LogInformation("There are no remaining pages to scrape");

                    yield break;
                }

                await context.ClearCookiesAsync(new() { Domain = _domain });
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
        await context.ClearCookiesAsync(new() { Domain = _domain });
        await page.GotoAsync(seriesUrl);

        var title = (await page.Locator("#titlemove > h1").TextContentAsync())!;
        var coverUrl = (await page
            .Locator("div:has(~ div:has-text('Bookmark')) > img").GetAttributeAsync("src"))!;

        return new PwScrapedSeries(page)
        {
            Title = title,
            CoverUrl = coverUrl,
            LatestChaptersAsync = ScrapeChaptersAsync(page),
        };
    }

    private async IAsyncEnumerable<string> ScrapeSeriesUrlsFromPageAsync(
        IPage page,
        HashSet<string> scrapedSeriesUrls)
    {
        var updateLocators = await page
            .Locator("div:has(> h2:has-text('Latest Update')) + div > div").AllAsync();

        foreach (var updateLocator in updateLocators)
        {
            var seriesUrl = (await updateLocator
                .Locator(":nth-match(a, 2)").GetAttributeAsync("href"))!;

            if (!scrapedSeriesUrls.Add(seriesUrl))
            {
                _logger.LogInformation("Pages shifted between iterations");

                continue;
            }

            yield return seriesUrl;
        }
    }

    private async IAsyncEnumerable<ScrapedChapter> ScrapeChaptersAsync(IPage page)
    {
        var scrapedChapterUrls = new HashSet<string>();
        var chapterLocators = await page.Locator("#chapterlist a").AllAsync();

        foreach (var chapterLocator in chapterLocators)
        {
            var url = (await chapterLocator.GetAttributeAsync("href"))!;

            if (!scrapedChapterUrls.Add(url))
            {
                _logger.LogInformation("Detected duplicated chapters in the list");

                continue;
            }

            var title = (await chapterLocator.Locator("> span:nth-of-type(1)").TextContentAsync())!;

            yield return new ScrapedChapter { Url = url, Title = title };
        }
    }
}
