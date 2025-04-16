namespace ScanlationTracker.Core.Scrapers.Dtos;

public class ScrapedSeries
{
    public required string Title { get; init; }

    public required string CoverUrl { get; init; }

    public required IEnumerable<ScrapedChapter> LatestChapters { get; init; }
}
