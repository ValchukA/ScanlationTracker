namespace ScanlationTracker.Core.Scrapers.Contracts;

public class ScrapedChapter
{
    public required string Url { get; init; }

    public required string Title { get; init; }
}
