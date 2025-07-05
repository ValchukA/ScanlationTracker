using ScanlationTracker.Infrastructure.UrlManagers;
using Xunit;

namespace ScanlationTracker.Infrastructure.Tests.UrlManagersTests;

public class RizzFablesUrlManagerTests
{
    public static IEnumerable<TheoryDataRow<string>> InvalidSeriesUrls { get; } =
    [
        new("Hello world"),
        new("https://example.com/series/series-1"),
        new("https://r.izz/series"),
        new("https://r.izz/not-series/series-1"),
    ];

    public static IEnumerable<TheoryDataRow<string>> InvalidChapterUrls { get; } =
    [
        new("Hello world"),
        new("https://example.com/chapter/series-1-chapter-3"),
        new("https://r.izz/chapter"),
        new("https://r.izz/not-chapter/series-1-chapter-3"),
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
        var seriesUrl = $"https://r.izz/series/series-1";
        var urlManager = new RizzFablesUrlManager("https://r.izz", "https://r.izz");

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
        var urlManager = new RizzFablesUrlManager("https://r.izz", "https://r.izz");

        // Act
        var action = () => urlManager.ExtractSeriesId(seriesUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ExtractChapterId_ReturnsChapterId()
    {
        // Arrange
        var chapterUrl = $"https://r.izz/chapter/series-1-chapter-3";
        var urlManager = new RizzFablesUrlManager("https://r.izz", "https://r.izz");

        // Act
        var chapterId = urlManager.ExtractChapterId(chapterUrl);

        // Assert
        Assert.Equal("series-1-chapter-3", chapterId);
    }

    [Theory]
    [MemberData(nameof(InvalidChapterUrls))]
    public void ExtractChapterId_ThrowsArgumentException_WhenChapterUrlIsInvalid(string chapterUrl)
    {
        // Arrange
        var urlManager = new RizzFablesUrlManager("https://r.izz", "https://r.izz");

        // Act
        var action = () => urlManager.ExtractChapterId(chapterUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ExtractRelativeCoverUrl_ReturnsRelativeCoverUrl()
    {
        // Arrange
        var coverUrl = $"https://r.izz/series-1.webp";
        var urlManager = new RizzFablesUrlManager("https://r.izz", "https://r.izz");

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
        var urlManager = new RizzFablesUrlManager("https://r.izz", "https://r.izz");

        // Act
        var action = () => urlManager.ExtractRelativeCoverUrl(coverUrl);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
}
