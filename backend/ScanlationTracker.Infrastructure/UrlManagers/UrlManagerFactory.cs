using ScanlationTracker.Core;
using ScanlationTracker.Core.UrlManagers;

namespace ScanlationTracker.Infrastructure.UrlManagers;

internal class UrlManagerFactory : IUrlManagerFactory
{
    public IUrlManager CreateUrlManager(ScanlationGroupName groupName, string baseWebsiteUrl)
        => groupName switch
        {
            ScanlationGroupName.AsuraScans => new AsuraScansUrlManager(baseWebsiteUrl),
            ScanlationGroupName.RizzFables => new RizzFablesUrlManager(baseWebsiteUrl),
            _ => throw new ArgumentException("Unexpected value"),
        };
}
