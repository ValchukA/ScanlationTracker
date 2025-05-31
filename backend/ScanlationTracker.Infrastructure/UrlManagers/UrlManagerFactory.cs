using ScanlationTracker.Core;
using ScanlationTracker.Core.UrlManagers;

namespace ScanlationTracker.Infrastructure.UrlManagers;

internal class UrlManagerFactory : IUrlManagerFactory
{
    public IUrlManager CreateUrlManager(ScanlationGroupName groupName, string baseWebsiteUrl, string baseCoverUrl)
        => groupName switch
        {
            ScanlationGroupName.AsuraScans => new AsuraScansUrlManager(baseWebsiteUrl, baseCoverUrl),
            ScanlationGroupName.RizzFables => new RizzFablesUrlManager(baseWebsiteUrl, baseCoverUrl),
            _ => throw new ArgumentException("Unexpected value"),
        };
}
