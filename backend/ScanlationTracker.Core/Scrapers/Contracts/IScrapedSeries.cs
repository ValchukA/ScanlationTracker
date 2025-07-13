namespace ScanlationTracker.Core.Scrapers.Contracts;

public interface IScrapedSeries : IAsyncDisposable
{
    public string Title { get; }

    public string CoverUrl { get; }

    public IAsyncEnumerable<ScrapedChapter> LatestChaptersAsync { get; }
}
