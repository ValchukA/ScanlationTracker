using ScanlationTracker.Core.Scrapers.Contracts;

namespace ScanlationTracker.Core.Scrapers;

public interface IScanlationScraper
{
    public IAsyncEnumerable<string> ScrapeUrlsOfLatestUpdatedSeriesAsync();

    public Task<IScrapedSeries> ScrapeSeriesAsync(string seriesUrl);
}
