namespace ScanlationTracker.Core.Scrapers.Dtos;

public class ScrapedSeriesUpdate
{
    public required string SeriesUrl { get; init; }

    public required string LatestChapterUrl { get; init; }
}
