namespace ScanlationTracker.Infrastructure.UrlManagers;

internal static class UrlExtractionHelper
{
    public static string ExtractLastSegmentFromValidUrl(
        string url,
        Uri baseUrl,
        int segmentsCount,
        (string Segment, int Index)[] requiredSegmentsWithIndices)
    {
        var urlValid = Uri.TryCreate(url.TrimEnd('/'), UriKind.Absolute, out var uri)
            && baseUrl.IsBaseOf(uri)
            && uri.Segments.Length == segmentsCount
            && requiredSegmentsWithIndices.All(SegmentExists);

        return urlValid
            ? uri!.Segments[^1]
            : throw new ArgumentException("Provided URL is not valid");

        bool SegmentExists((string Segment, int Index) segmentWithIndex)
            => uri.Segments[segmentWithIndex.Index] == $"{segmentWithIndex.Segment}/";
    }

    public static string ExtractRelativeUrlFromValidUrl(string url, Uri baseUrl)
    {
        var urlValid = Uri.TryCreate(url, UriKind.Absolute, out var uri) && baseUrl.IsBaseOf(uri);

        return urlValid
            ? uri!.AbsolutePath
            : throw new ArgumentException("Provided URL is not valid");
    }
}
