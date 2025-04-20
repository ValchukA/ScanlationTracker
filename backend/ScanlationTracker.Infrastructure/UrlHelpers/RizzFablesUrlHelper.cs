namespace ScanlationTracker.Infrastructure.UrlHelpers;

internal class RizzFablesUrlHelper
{
    public RizzFablesUrlHelper(string baseWebsiteUrl) => LatestUpdatesUrl = baseWebsiteUrl;

    public string LatestUpdatesUrl { get; }
}
