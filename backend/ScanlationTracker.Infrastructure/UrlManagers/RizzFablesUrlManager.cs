using ScanlationTracker.Core.UrlManagers;

namespace ScanlationTracker.Infrastructure.UrlManagers;

internal class RizzFablesUrlManager : IUrlManager
{
    private const string _seriesSegment = "series";
    private const string _chapterSegment = "chapter";

    private readonly Uri _baseWebsiteUrl;
    private readonly Uri _baseCoverUrl;

    public RizzFablesUrlManager(string baseWebsiteUrl, string baseCoverUrl)
    {
        _baseWebsiteUrl = new Uri(baseWebsiteUrl);
        _baseCoverUrl = new Uri(baseCoverUrl);
        LatestUpdatesUrl = baseWebsiteUrl;
    }

    public string LatestUpdatesUrl { get; }

    public string ExtractSeriesId(string seriesUrl)
        => UrlExtractionHelper.ExtractLastSegmentFromValidUrl(
            seriesUrl,
            _baseWebsiteUrl,
            3,
            [(_seriesSegment, 1)]);

    public string ExtractChapterId(string chapterUrl)
        => UrlExtractionHelper.ExtractLastSegmentFromValidUrl(
            chapterUrl,
            _baseWebsiteUrl,
            3,
            [(_chapterSegment, 1)]);

    public string ExtractRelativeCoverUrl(string coverUrl)
        => UrlExtractionHelper.ExtractRelativeUrlFromValidUrl(coverUrl, _baseCoverUrl);
}
