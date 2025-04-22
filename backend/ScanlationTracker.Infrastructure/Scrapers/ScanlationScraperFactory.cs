using Microsoft.Extensions.Logging;
using ScanlationTracker.Core;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Infrastructure.UrlManagers;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal class ScanlationScraperFactory : IScanlationScraperFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;

    public ScanlationScraperFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
    }

    public IScanlationScraper CreateScraper(ScanlationGroupName groupName, string baseWebsiteUrl)
        => groupName switch
        {
            ScanlationGroupName.AsuraScans => CreateAsuraScansScraper(baseWebsiteUrl),
            ScanlationGroupName.RizzFables => CreateRizzFablesScraper(baseWebsiteUrl),
            ScanlationGroupName.ReaperScans => new ReaperScansAsScraper(),
            _ => throw new ArgumentException("Unexpected value"),
        };

    private AsuraScansAsScraper CreateAsuraScansScraper(string baseWebsiteUrl)
    {
        var urlManager = new AsuraScansUrlManager(baseWebsiteUrl);
        var logger = _loggerFactory.CreateLogger<AsuraScansAsScraper>();

        return new AsuraScansAsScraper(_httpClientFactory, urlManager, logger);
    }

    private RizzFablesAsScraper CreateRizzFablesScraper(string baseWebsiteUrl)
    {
        var urlManager = new RizzFablesUrlManager(baseWebsiteUrl);
        var logger = _loggerFactory.CreateLogger<RizzFablesAsScraper>();

        return new RizzFablesAsScraper(_httpClientFactory, urlManager, logger);
    }
}
