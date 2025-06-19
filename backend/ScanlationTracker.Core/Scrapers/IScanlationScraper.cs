using ScanlationTracker.Core.Scrapers.Dtos;

namespace ScanlationTracker.Core.Scrapers;

public interface IScanlationScraper
{
    public IAsyncEnumerable<string> ScrapeUrlsOfLatestUpdatedSeriesAsync();

    public Task<IScrapedSeries> ScrapeSeriesAsync(string seriesUrl);
}
