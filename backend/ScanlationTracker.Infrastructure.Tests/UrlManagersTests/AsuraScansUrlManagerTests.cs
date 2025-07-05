using ScanlationTracker.Infrastructure.UrlManagers;
using Xunit;

namespace ScanlationTracker.Infrastructure.Tests.UrlManagersTests;

public class AsuraScansUrlManagerTests
{
    public static IEnumerable<TheoryDataRow<string>> InvalidSeriesUrls { get; } =
    [
        new("Hello world"),
        new("https://example.com/series/series-1"),
        new("https://asu.ra/series"),
        new("https://asu.ra/not-series/series-1"),
    ];

    public static IEnumerable<TheoryDataRow<string>> InvalidChapterUrls { get; } =
    [
        new("Hello world"),
        new("https://example.com/series/series-1/chapter/3"),
        new("https://asu.ra/series/chapter"),
        new("https://asu.ra/not-series/series-1/chapter/3"),
        new("https://asu.ra/series/series-1/not-chapter/3"),
    ];

    public static IEnumerable<TheoryDataRow<string>> InvalidCoverUrls { get; } =
    [
        new("Hello world"),
        new("https://example.com/series-1.webp"),
    ];

    [Fact]
    public void ExtractSeriesId_ReturnsSeriesId()
    {
        // Arrange
        var seriesUrl = $"https://asu.ra/series/series-1";
        var urlManager = new AsuraScansUrlManager("https://asu.ra", "https://gg.asu.ra");

        // Act
        var seriesId = urlManager.ExtractSeriesId(seriesUrl);

        // Assert
        Assert.Equal("series-1", seriesId);
    }

    [Theory]
    [MemberData(nameof(InvalidSeriesUrls))]
    public void ExtractSeriesId_ThrowsArgumentException_WhenSeriesUrlIsInvalid(string seriesUrl)
    {
        // Arrange
        var urlManager = new AsuraScansUrlManager("https://asu.ra", "https://gg.asu.ra");

        // Act
        var action = () => urlManager.ExtractSeriesId(seriesUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ExtractChapterId_ReturnsChapterId()
    {
        // Arrange
        var chapterUrl = $"https://asu.ra/series/series-1/chapter/3";
        var urlManager = new AsuraScansUrlManager("https://asu.ra", "https://gg.asu.ra");

        // Act
        var chapterId = urlManager.ExtractChapterId(chapterUrl);

        // Assert
        Assert.Equal("3", chapterId);
    }

    [Theory]
    [MemberData(nameof(InvalidChapterUrls))]
    public void ExtractChapterId_ThrowsArgumentException_WhenChapterUrlIsInvalid(string chapterUrl)
    {
        // Arrange
        var urlManager = new AsuraScansUrlManager("https://asu.ra", "https://gg.asu.ra");

        // Act
        var action = () => urlManager.ExtractChapterId(chapterUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ExtractRelativeCoverUrl_ReturnsRelativeCoverUrl()
    {
        // Arrange
        var coverUrl = $"https://gg.asu.ra/series-1.webp";
        var urlManager = new AsuraScansUrlManager("https://asu.ra", "https://gg.asu.ra");

        // Act
        var relativeCoverUrl = urlManager.ExtractRelativeCoverUrl(coverUrl);

        // Assert
        Assert.Equal("/series-1.webp", relativeCoverUrl);
    }

    [Theory]
    [MemberData(nameof(InvalidCoverUrls))]
    public void ExtractRelativeCoverUrl_ThrowsArgumentException_WhenCoverUrlIsInvalid(string coverUrl)
    {
        // Arrange
        var urlManager = new AsuraScansUrlManager("https://asu.ra", "https://gg.asu.ra");

        // Act
        var action = () => urlManager.ExtractRelativeCoverUrl(coverUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
}
