using ScanlationTracker.Core.UrlManagers;

namespace ScanlationTracker.Infrastructure.UrlManagers;

internal class AsuraScansUrlManager : IUrlManager
{
    public AsuraScansUrlManager(string baseWebsiteUrl) => LatestUpdatesUrl = baseWebsiteUrl;

    public string LatestUpdatesUrl { get; }
}
