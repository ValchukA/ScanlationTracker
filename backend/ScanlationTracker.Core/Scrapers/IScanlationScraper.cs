﻿using ScanlationTracker.Core.Scrapers.Dtos;

namespace ScanlationTracker.Core.Scrapers;

public interface IScanlationScraper
{
    public IAsyncEnumerable<ScrapedSeriesUpdate> ScrapeLatestUpdatesAsync();

    public Task<ScrapedSeries> ScrapeSeriesAsync(string seriesUrl);
}
