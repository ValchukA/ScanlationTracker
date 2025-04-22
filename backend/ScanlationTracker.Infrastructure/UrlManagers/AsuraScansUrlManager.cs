namespace ScanlationTracker.Infrastructure.UrlManagers;

internal class AsuraScansUrlManager
{
    public AsuraScansUrlManager(string baseWebsiteUrl) => LatestUpdatesUrl = baseWebsiteUrl;

    public string LatestUpdatesUrl { get; }
}
