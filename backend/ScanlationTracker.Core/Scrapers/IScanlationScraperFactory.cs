namespace ScanlationTracker.Core.Scrapers;

public interface IScanlationScraperFactory
{
    public IScanlationScraper CreateScraper(ScanlationGroupName groupName, string baseWebsiteUrl);
}
