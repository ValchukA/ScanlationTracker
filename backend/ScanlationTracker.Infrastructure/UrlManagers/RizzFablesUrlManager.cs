namespace ScanlationTracker.Infrastructure.UrlManagers;

internal class RizzFablesUrlManager
{
    public RizzFablesUrlManager(string baseWebsiteUrl) => LatestUpdatesUrl = baseWebsiteUrl;

    public string LatestUpdatesUrl { get; }
}
