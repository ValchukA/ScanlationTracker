namespace ScanlationTracker.Core.UrlManagers;

public interface IUrlManager
{
    public string LatestUpdatesUrl { get; }

    public string ExtractSeriesId(string seriesUrl);

    public string ExtractChapterId(string chapterUrl);

    public string ExtractRelativeCoverUrl(string coverUrl);
}
