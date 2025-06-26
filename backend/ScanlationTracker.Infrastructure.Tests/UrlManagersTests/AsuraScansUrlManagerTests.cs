using ScanlationTracker.Infrastructure.UrlManagers;
using Xunit;

namespace ScanlationTracker.Infrastructure.Tests.UrlManagersTests;

public class AsuraScansUrlManagerTests
{
    private const string _baseWebsiteUrl = "https://asuracomic.net";
    private const string _baseCoverUrl = "https://gg.asuracomic.net";

    public static IEnumerable<TheoryDataRow<string>> GetInvalidSeriesUrls()
    {
        yield return new TheoryDataRow<string>(
            "Hello world");

        yield return new TheoryDataRow<string>(
            "https://example.com/series/the-return-of-the-crazy-demon-62275c85");

        yield return new TheoryDataRow<string>(
            $"{_baseWebsiteUrl}/series");

        yield return new TheoryDataRow<string>(
            $"{_baseWebsiteUrl}/not-series/the-return-of-the-crazy-demon-62275c85");
    }

    public static IEnumerable<TheoryDataRow<string>> GetInvalidChapterUrls()
    {
        yield return new TheoryDataRow<string>(
            "Hello world");

        yield return new TheoryDataRow<string>(
            "https://example.com/series/the-return-of-the-crazy-demon-62275c85/chapter/159");

        yield return new TheoryDataRow<string>(
            $"{_baseWebsiteUrl}/series/chapter");

        yield return new TheoryDataRow<string>(
            $"{_baseWebsiteUrl}/not-series/the-return-of-the-crazy-demon-62275c85/chapter/159");

        yield return new TheoryDataRow<string>(
            $"{_baseWebsiteUrl}/series/the-return-of-the-crazy-demon-62275c85/not-chapter/159");
    }

    public static IEnumerable<TheoryDataRow<string>> GetInvalidCoverUrls()
    {
        yield return new TheoryDataRow<string>(
            "Hello world");

        yield return new TheoryDataRow<string>(
            "https://example.com/storage/media/79/1be5e62f.webp");
    }

    [Fact]
    public void ExtractSeriesId_ReturnsSeriesId()
    {
        // Arrange
        var seriesUrl = $"{_baseWebsiteUrl}/series/the-return-of-the-crazy-demon-62275c85";
        var urlManager = new AsuraScansUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var seriesId = urlManager.ExtractSeriesId(seriesUrl);

        // Assert
        Assert.Equal("the-return-of-the-crazy-demon-62275c85", seriesId);
    }

    [Theory]
    [MemberData(nameof(GetInvalidSeriesUrls))]
    public void ExtractSeriesId_ThrowsArgumentException_WhenSeriesUrlIsInvalid(string seriesUrl)
    {
        // Arrange
        var urlManager = new AsuraScansUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var action = () => urlManager.ExtractSeriesId(seriesUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ExtractChapterId_ReturnsChapterId()
    {
        // Arrange
        var chapterUrl = $"{_baseWebsiteUrl}/series/the-return-of-the-crazy-demon-62275c85/chapter/159";
        var urlManager = new AsuraScansUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var chapterId = urlManager.ExtractChapterId(chapterUrl);

        // Assert
        Assert.Equal("159", chapterId);
    }

    [Theory]
    [MemberData(nameof(GetInvalidChapterUrls))]
    public void ExtractChapterId_ThrowsArgumentException_WhenChapterUrlIsInvalid(string chapterUrl)
    {
        // Arrange
        var urlManager = new AsuraScansUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var action = () => urlManager.ExtractChapterId(chapterUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ExtractRelativeCoverUrl_ReturnsRelativeCoverUrl()
    {
        // Arrange
        var coverUrl = $"{_baseCoverUrl}/storage/media/79/1be5e62f.webp";
        var urlManager = new AsuraScansUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var relativeCoverUrl = urlManager.ExtractRelativeCoverUrl(coverUrl);

        // Assert
        Assert.Equal("/storage/media/79/1be5e62f.webp", relativeCoverUrl);
    }

    [Theory]
    [MemberData(nameof(GetInvalidCoverUrls))]
    public void ExtractRelativeCoverUrl_ThrowsArgumentException_WhenCoverUrlIsInvalid(string coverUrl)
    {
        // Arrange
        var urlManager = new AsuraScansUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var action = () => urlManager.ExtractRelativeCoverUrl(coverUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
}
