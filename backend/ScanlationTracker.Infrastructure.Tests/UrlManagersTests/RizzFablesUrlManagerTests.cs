using ScanlationTracker.Infrastructure.UrlManagers;
using Xunit;

namespace ScanlationTracker.Infrastructure.Tests.UrlManagersTests;

public class RizzFablesUrlManagerTests
{
    private const string _baseWebsiteUrl = "https://rizzfables.com";
    private const string _baseCoverUrl = "https://rizzfables.com";

    public static IEnumerable<TheoryDataRow<string>> GetInvalidSeriesUrls()
    {
        yield return new TheoryDataRow<string>(
            "Hello world");

        yield return new TheoryDataRow<string>(
            "https://example.com/series/r2311170-top-tier-providence");

        yield return new TheoryDataRow<string>(
            $"{_baseWebsiteUrl}/series");

        yield return new TheoryDataRow<string>(
            $"{_baseWebsiteUrl}/not-series/r2311170-top-tier-providence");
    }

    public static IEnumerable<TheoryDataRow<string>> GetInvalidChapterUrls()
    {
        yield return new TheoryDataRow<string>(
            "Hello world");

        yield return new TheoryDataRow<string>(
            "https://example.com/chapter/r2311170-top-tier-providence-chapter-220");

        yield return new TheoryDataRow<string>(
            $"{_baseWebsiteUrl}/chapter");

        yield return new TheoryDataRow<string>(
            $"{_baseWebsiteUrl}/not-chapter/r2311170-top-tier-providence-chapter-220");
    }

    public static IEnumerable<TheoryDataRow<string>> GetInvalidCoverUrls()
    {
        yield return new TheoryDataRow<string>(
            "Hello world");

        yield return new TheoryDataRow<string>(
            "https://example.com/assets/images/ttp11.webp");
    }

    [Fact]
    public void ExtractSeriesId_ReturnsSeriesId()
    {
        // Arrange
        var seriesUrl = $"{_baseWebsiteUrl}/series/r2311170-top-tier-providence";
        var urlManager = new RizzFablesUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var seriesId = urlManager.ExtractSeriesId(seriesUrl);

        // Assert
        Assert.Equal("r2311170-top-tier-providence", seriesId);
    }

    [Theory]
    [MemberData(nameof(GetInvalidSeriesUrls))]
    public void ExtractSeriesId_ThrowsArgumentException_WhenSeriesUrlIsInvalid(string seriesUrl)
    {
        // Arrange
        var urlManager = new RizzFablesUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var action = () => urlManager.ExtractSeriesId(seriesUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ExtractChapterId_ReturnsChapterId()
    {
        // Arrange
        var chapterUrl = $"{_baseWebsiteUrl}/chapter/r2311170-top-tier-providence-chapter-220";
        var urlManager = new RizzFablesUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var chapterId = urlManager.ExtractChapterId(chapterUrl);

        // Assert
        Assert.Equal("r2311170-top-tier-providence-chapter-220", chapterId);
    }

    [Theory]
    [MemberData(nameof(GetInvalidChapterUrls))]
    public void ExtractChapterId_ThrowsArgumentException_WhenChapterUrlIsInvalid(string chapterUrl)
    {
        // Arrange
        var urlManager = new RizzFablesUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var action = () => urlManager.ExtractChapterId(chapterUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ExtractRelativeCoverUrl_ReturnsRelativeCoverUrl()
    {
        // Arrange
        var coverUrl = $"{_baseCoverUrl}/assets/images/ttp11.webp";
        var urlManager = new RizzFablesUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var relativeCoverUrl = urlManager.ExtractRelativeCoverUrl(coverUrl);

        // Assert
        Assert.Equal("/assets/images/ttp11.webp", relativeCoverUrl);
    }

    [Theory]
    [MemberData(nameof(GetInvalidCoverUrls))]
    public void ExtractRelativeCoverUrl_ThrowsArgumentException_WhenCoverUrlIsInvalid(string coverUrl)
    {
        // Arrange
        var urlManager = new RizzFablesUrlManager(_baseWebsiteUrl, _baseCoverUrl);

        // Act
        var action = () => urlManager.ExtractRelativeCoverUrl(coverUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
}
