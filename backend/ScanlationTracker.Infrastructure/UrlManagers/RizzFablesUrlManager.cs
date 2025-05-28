using ScanlationTracker.Core.UrlManagers;

namespace ScanlationTracker.Infrastructure.UrlManagers;

internal class RizzFablesUrlManager : IUrlManager
{
    public RizzFablesUrlManager(string baseWebsiteUrl) => LatestUpdatesUrl = baseWebsiteUrl;

    public string LatestUpdatesUrl { get; }
}
