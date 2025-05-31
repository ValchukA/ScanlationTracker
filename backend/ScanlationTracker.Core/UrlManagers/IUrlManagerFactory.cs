namespace ScanlationTracker.Core.UrlManagers;

public interface IUrlManagerFactory
{
    public IUrlManager CreateUrlManager(ScanlationGroupName groupName, string baseWebsiteUrl, string baseCoverUrl);
}
