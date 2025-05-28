using Microsoft.Playwright;
using ScanlationTracker.Core.Scrapers.Dtos;

namespace ScanlationTracker.Infrastructure.Scrapers;

internal class PwScrapedSeries : IScrapedSeries
{
    private readonly IPage _page;

    public PwScrapedSeries(IPage page) => _page = page;

    public required string Title { get; init; }

    public required string CoverUrl { get; init; }

    public required IAsyncEnumerable<ScrapedChapter> LatestChaptersAsync { get; init; }

    public async ValueTask DisposeAsync() => await _page.CloseAsync();
}
