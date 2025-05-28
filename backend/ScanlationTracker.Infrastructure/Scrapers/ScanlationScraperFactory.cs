using Microsoft.Extensions.Logging;
using ScanlationTracker.Core;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Infrastructure.Scrapers.BrowserContext;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal class ScanlationScraperFactory : IScanlationScraperFactory
{
    private readonly IPwBrowserContextHolder _pwBrowserContextHolder;
    private readonly ILoggerFactory _loggerFactory;

    public ScanlationScraperFactory(IPwBrowserContextHolder pwBrowserContextHolder, ILoggerFactory loggerFactory)
    {
        _pwBrowserContextHolder = pwBrowserContextHolder;
        _loggerFactory = loggerFactory;
    }

    public IScanlationScraper CreateScraper(ScanlationGroupName groupName, string latestUpdatesUrl)
        => groupName switch
        {
            ScanlationGroupName.AsuraScans => CreateAsuraScansScraper(latestUpdatesUrl),
            ScanlationGroupName.RizzFables => CreateRizzFablesScraper(latestUpdatesUrl),
            _ => throw new ArgumentException("Unexpected value"),
        };

    private AsuraScansPwScraper CreateAsuraScansScraper(string latestUpdatesUrl)
    {
        var logger = _loggerFactory.CreateLogger<AsuraScansPwScraper>();

        return new AsuraScansPwScraper(_pwBrowserContextHolder, latestUpdatesUrl, logger);
    }

    private RizzFablesPwScraper CreateRizzFablesScraper(string latestUpdatesUrl)
    {
        var logger = _loggerFactory.CreateLogger<RizzFablesPwScraper>();

        return new RizzFablesPwScraper(_pwBrowserContextHolder, latestUpdatesUrl, logger);
    }
}
