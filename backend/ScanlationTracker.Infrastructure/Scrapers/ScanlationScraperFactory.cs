using Microsoft.Extensions.Logging;
using ScanlationTracker.Core;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Infrastructure.UrlHelpers;

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
        var httpClient = _httpClientFactory.CreateClient();
        var urlHelper = new AsuraScansUrlHelper(baseWebsiteUrl);
        var logger = _loggerFactory.CreateLogger<AsuraScansAsScraper>();

        return new AsuraScansAsScraper(httpClient, urlHelper, logger);
    }

    private RizzFablesAsScraper CreateRizzFablesScraper(string baseWebsiteUrl)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var urlHelper = new RizzFablesUrlHelper(baseWebsiteUrl);
        var logger = _loggerFactory.CreateLogger<RizzFablesAsScraper>();

        return new RizzFablesAsScraper(httpClient, urlHelper, logger);
    }
}
