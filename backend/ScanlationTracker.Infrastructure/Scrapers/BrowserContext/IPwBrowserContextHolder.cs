using Microsoft.Playwright;

namespace ScanlationTracker.Infrastructure.Scrapers.BrowserContext;

internal interface IPwBrowserContextHolder
{
    public Task<IBrowserContext> GetContextAsync();
}
