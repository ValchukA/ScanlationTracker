using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.Scrapers.Dtos;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal class ReaperScansAsScraper : IScanlationScraper
{
    public async IAsyncEnumerable<ScrapedSeriesUpdate> ScrapeLatestUpdatesAsync()
    {
        await Task.CompletedTask;

        yield break;
    }

    public Task<ScrapedSeries> ScrapeSeriesAsync(string seriesUrl) => throw new NotImplementedException();
}
