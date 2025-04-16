namespace ScanlationTracker.Infrastructure.UrlHelpers;

internal class AsuraScansUrlHelper
{
    public AsuraScansUrlHelper(string baseWebsiteUrl) => LatestUpdatesUrl = baseWebsiteUrl;

    public string LatestUpdatesUrl { get; }
}
