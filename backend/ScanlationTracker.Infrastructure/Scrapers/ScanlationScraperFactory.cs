using Microsoft.Extensions.Logging;
using ScanlationTracker.Core;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Infrastructure.UrlHelpers;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal class ScanlationScraperFactory : IScanlationScraperFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public ScanlationScraperFactory(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

    public IScanlationScraper CreateScraper(ScanlationGroupName groupName, string baseWebsiteUrl)
        => groupName switch
        {
            ScanlationGroupName.AsuraScans => CreateAsuraScansScraper(baseWebsiteUrl),
            ScanlationGroupName.RizzFables => new RizzFablesAsScraper(),
            ScanlationGroupName.ReaperScans => new ReaperScansAsScraper(),
            _ => throw new ArgumentException("Unexpected value"),
        };

    private AsuraScansAsScraper CreateAsuraScansScraper(string baseWebsiteUrl)
    {
        var urlHelper = new AsuraScansUrlHelper(baseWebsiteUrl);
        var logger = _loggerFactory.CreateLogger<AsuraScansAsScraper>();

        return new AsuraScansAsScraper(urlHelper, logger);
    }
}
