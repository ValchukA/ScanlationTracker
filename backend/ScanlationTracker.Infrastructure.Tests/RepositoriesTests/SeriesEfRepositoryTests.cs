using Microsoft.EntityFrameworkCore;
using ScanlationTracker.Core;
using ScanlationTracker.Core.Repositories.Dtos;
using ScanlationTracker.Infrastructure.Database;
using ScanlationTracker.Infrastructure.Database.Entities;
using ScanlationTracker.Infrastructure.Database.Repositories;
using Xunit;

namespace ScanlationTracker.Infrastructure.Tests.RepositoriesTests;

public sealed class SeriesEfRepositoryTests : IAsyncLifetime, IClassFixture<PostgresFixture>
{
    private readonly ScanlationDbContext _dbContext;
    private readonly SeriesEfRepository _seriesRepository;

    public SeriesEfRepositoryTests(PostgresFixture postgresFixture)
    {
        var dbContextOptions = new DbContextOptionsBuilder<ScanlationDbContext>()
            .UseNpgsql(postgresFixture.Container.GetConnectionString())
            .Options;

        _dbContext = new ScanlationDbContext(dbContextOptions);
        _seriesRepository = new SeriesEfRepository(_dbContext);
    }

    public static IEnumerable<TheoryDataRow<ChapterDto, ChapterDto>> GetChaptersViolatingUniqueConstraint()
    {
        var firstChapter = new ChapterDto()
        {
            Id = Guid.Parse("0197eb79-24d6-7063-a73d-f4df3b320f68"),
            SeriesId = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            ExternalId = "1",
            Title = "Chapter 1",
            Number = 1,
            AddedAt = new DateTimeOffset(2025, 7, 8, 0, 0, 0, default),
        };

        yield return new TheoryDataRow<ChapterDto, ChapterDto>(
            firstChapter,
            new()
            {
                Id = Guid.Parse("0197eb79-24d6-742e-8696-fa6536a119af"),
                SeriesId = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
                ExternalId = "1",
                Title = "Chapter 2",
                Number = 2,
                AddedAt = new DateTimeOffset(2025, 7, 8, 0, 0, 0, default),
            });

        yield return new TheoryDataRow<ChapterDto, ChapterDto>(
            firstChapter,
            new()
            {
                Id = Guid.Parse("0197eb79-24d6-742e-8696-fa6536a119af"),
                SeriesId = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
                ExternalId = "2",
                Title = "Chapter 2",
                Number = 1,
                AddedAt = new DateTimeOffset(2025, 7, 8, 0, 0, 0, default),
            });
    }

    public async ValueTask InitializeAsync() => await _dbContext.Database.MigrateAsync();

    public async ValueTask DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetAllGroupsAsync_ReturnsAllGroups()
    {
        // Arrange
        var groupsToSeed = new ScanlationGroupEntity[]
        {
            new()
            {
                Id = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
                Name = GroupNameConstants.AsuraScans,
                PublicName = "Asura Scans",
                BaseWebsiteUrl = "https://asu.ra",
                BaseCoverUrl = "https://gg.asu.ra",
            },
            new()
            {
                Id = Guid.Parse("0197e151-db58-7d54-959f-d758d850130a"),
                Name = GroupNameConstants.RizzFables,
                PublicName = "Rizz Fables",
                BaseWebsiteUrl = "https://r.izz",
                BaseCoverUrl = "https://r.izz",
            },
        };

        _dbContext.ScanlationGroups.AddRange(groupsToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        // Act
        var groups = await _seriesRepository.GetAllGroupsAsync();

        // Assert
        var expectedGroups = new ScanlationGroupDto[]
        {
            new()
            {
                Id = groupsToSeed[0].Id,
                Name = ScanlationGroupName.AsuraScans,
                PublicName = groupsToSeed[0].PublicName,
                BaseWebsiteUrl = groupsToSeed[0].BaseWebsiteUrl,
                BaseCoverUrl = groupsToSeed[0].BaseCoverUrl,
            },
            new()
            {
                Id = groupsToSeed[1].Id,
                Name = ScanlationGroupName.RizzFables,
                PublicName = groupsToSeed[1].PublicName,
                BaseWebsiteUrl = groupsToSeed[1].BaseWebsiteUrl,
                BaseCoverUrl = groupsToSeed[1].BaseCoverUrl,
            },
        };

        Assert.Equal(expectedGroups, groups);
    }

    [Fact]
    public async Task GetSeriesByExternalIdAsync_ReturnsSeries()
    {
        // Arrange
        var groupToSeed = new ScanlationGroupEntity()
        {
            Id = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
            Name = GroupNameConstants.AsuraScans,
            PublicName = "Asura Scans",
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var seriesToSeed = new SeriesEntity()
        {
            Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            ScanlationGroupId = groupToSeed.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.Add(seriesToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        // Act
        var series = await _seriesRepository.GetSeriesByExternalIdAsync(
            groupToSeed.Id,
            seriesToSeed.ExternalId);

        // Assert
        var expectedSeries = new SeriesDto
        {
            Id = seriesToSeed.Id,
            ScanlationGroupId = seriesToSeed.ScanlationGroupId,
            ExternalId = seriesToSeed.ExternalId,
            Title = seriesToSeed.Title,
            RelativeCoverUrl = seriesToSeed.RelativeCoverUrl,
        };

        Assert.Equal(expectedSeries, series);
    }

    [Fact]
    public async Task GetSeriesByExternalIdAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var series = await _seriesRepository.GetSeriesByExternalIdAsync(Guid.Empty, string.Empty);

        // Assert
        Assert.Null(series);
    }

    [Fact]
    public async Task GetSeriesByTitleAsync_ReturnsSeries()
    {
        // Arrange
        var groupToSeed = new ScanlationGroupEntity()
        {
            Id = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
            Name = GroupNameConstants.AsuraScans,
            PublicName = "Asura Scans",
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var seriesToSeed = new SeriesEntity()
        {
            Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            ScanlationGroupId = groupToSeed.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.Add(seriesToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        // Act
        var series = await _seriesRepository.GetSeriesByTitleAsync(
            groupToSeed.Id,
            seriesToSeed.Title);

        // Assert
        var expectedSeries = new SeriesDto
        {
            Id = seriesToSeed.Id,
            ScanlationGroupId = seriesToSeed.ScanlationGroupId,
            ExternalId = seriesToSeed.ExternalId,
            Title = seriesToSeed.Title,
            RelativeCoverUrl = seriesToSeed.RelativeCoverUrl,
        };

        Assert.Equal(expectedSeries, series);
    }

    [Fact]
    public async Task GetSeriesByTitleAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var series = await _seriesRepository.GetSeriesByTitleAsync(Guid.Empty, string.Empty);

        // Assert
        Assert.Null(series);
    }

    [Fact]
    public async Task GetLatestChapterAsync_ReturnsLatestChapter()
    {
        // Arrange
        var groupToSeed = new ScanlationGroupEntity()
        {
            Id = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
            Name = GroupNameConstants.AsuraScans,
            PublicName = "Asura Scans",
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var seriesToSeed = new SeriesEntity()
        {
            Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            ScanlationGroupId = groupToSeed.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        var chaptersToSeed = new ChapterEntity[]
        {
            new()
            {
                Id = Guid.Parse("0197eb79-24d6-7448-b5fa-cc8f871c4244"),
                SeriesId = seriesToSeed.Id,
                ExternalId = "3",
                Title = "Chapter 3",
                Number = 3,
                AddedAt = new DateTimeOffset(2025, 7, 8, 0, 0, 0, default),
            },
            new()
            {
                Id = Guid.Parse("0197eb79-24d6-742e-8696-fa6536a119af"),
                SeriesId = seriesToSeed.Id,
                ExternalId = "2",
                Title = "Chapter 2",
                Number = 2,
                AddedAt = new DateTimeOffset(2025, 7, 8, 0, 0, 0, default),
            },
            new()
            {
                Id = Guid.Parse("0197eb79-24d6-7063-a73d-f4df3b320f68"),
                SeriesId = seriesToSeed.Id,
                ExternalId = "1",
                Title = "Chapter 1",
                Number = 1,
                AddedAt = new DateTimeOffset(2025, 7, 8, 0, 0, 0, default),
            },
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.Add(seriesToSeed);
        _dbContext.Chapters.AddRange(chaptersToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        // Act
        var latestChapter = await _seriesRepository.GetLatestChapterAsync(seriesToSeed.Id);

        // Assert
        var expectedLatestChapter = new ChapterDto
        {
            Id = chaptersToSeed[0].Id,
            SeriesId = chaptersToSeed[0].SeriesId,
            ExternalId = chaptersToSeed[0].ExternalId,
            Title = chaptersToSeed[0].Title,
            Number = chaptersToSeed[0].Number,
            AddedAt = chaptersToSeed[0].AddedAt,
        };

        Assert.Equal(expectedLatestChapter, latestChapter);
    }

    [Fact]
    public async Task GetLatestChapterAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var latestChapter = await _seriesRepository.GetLatestChapterAsync(Guid.Empty);

        // Assert
        Assert.Null(latestChapter);
    }

    [Fact]
    public async Task AddSeries_AddsSeries()
    {
        // Arrange
        var groupToSeed = new ScanlationGroupEntity()
        {
            Id = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
            Name = GroupNameConstants.AsuraScans,
            PublicName = "Asura Scans",
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        var seriesToAdd = new SeriesDto()
        {
            Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            ScanlationGroupId = groupToSeed.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        // Act
        _seriesRepository.AddSeries(seriesToAdd);
        await _seriesRepository.SaveChangesAsync();

        // Assert
        _dbContext.ChangeTracker.Clear();

        var addedSeries = await _dbContext.Series.FirstAsync(
            series => series.Id == seriesToAdd.Id,
            TestContext.Current.CancellationToken);

        var expectedAddedSeries = new SeriesEntity
        {
            Id = seriesToAdd.Id,
            ScanlationGroupId = seriesToAdd.ScanlationGroupId,
            ExternalId = seriesToAdd.ExternalId,
            Title = seriesToAdd.Title,
            RelativeCoverUrl = seriesToAdd.RelativeCoverUrl,
        };

        Assert.Equivalent(expectedAddedSeries, addedSeries, true);
    }

    [Fact]
    public async Task AddSeries_ThrowsException_WhenUniqueConstraintIsViolated()
    {
        // Arrange
        var groupToSeed = new ScanlationGroupEntity()
        {
            Id = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
            Name = GroupNameConstants.AsuraScans,
            PublicName = "Asura Scans",
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        var seriesToAdd = new SeriesDto[]
        {
            new()
            {
                Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
                ScanlationGroupId = groupToSeed.Id,
                ExternalId = "series-1",
                Title = "Series 2",
                RelativeCoverUrl = "/series-2.webp",
            },
            new()
            {
                Id = Guid.Parse("0197ebbb-682b-7a08-aa52-cad3d0a9294e"),
                ScanlationGroupId = groupToSeed.Id,
                ExternalId = "series-1",
                Title = "Series 1",
                RelativeCoverUrl = "/series-1.webp",
            },
        };

        // Act
        _seriesRepository.AddSeries(seriesToAdd[1]);
        _seriesRepository.AddSeries(seriesToAdd[0]);
        var exception = await Record.ExceptionAsync(_seriesRepository.SaveChangesAsync);

        // Assert
        Assert.Contains(
            "duplicate key value violates unique constraint",
            exception!.InnerException!.Message);
    }

    [Fact]
    public async Task UpdateSeries_UpdatesSeries_WhenNotTracked()
    {
        // Arrange
        var groupToSeed = new ScanlationGroupEntity()
        {
            Id = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
            Name = GroupNameConstants.AsuraScans,
            PublicName = "Asura Scans",
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var seriesToSeed = new SeriesEntity()
        {
            Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            ScanlationGroupId = groupToSeed.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.Add(seriesToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        var seriesToUpdate = new SeriesDto()
        {
            Id = seriesToSeed.Id,
            ScanlationGroupId = seriesToSeed.ScanlationGroupId,
            ExternalId = "series-1-updated",
            Title = "Series 1 Updated",
            RelativeCoverUrl = "/series-1-updated.webp",
        };

        // Act
        _seriesRepository.UpdateSeries(seriesToUpdate);
        await _seriesRepository.SaveChangesAsync();

        // Assert
        _dbContext.ChangeTracker.Clear();

        var updatedSeries = await _dbContext.Series.FirstAsync(
            series => series.Id == seriesToUpdate.Id,
            TestContext.Current.CancellationToken);

        var expectedUpdatedSeries = new SeriesEntity
        {
            Id = seriesToUpdate.Id,
            ScanlationGroupId = seriesToUpdate.ScanlationGroupId,
            ExternalId = seriesToUpdate.ExternalId,
            Title = seriesToUpdate.Title,
            RelativeCoverUrl = seriesToUpdate.RelativeCoverUrl,
        };

        Assert.Equivalent(expectedUpdatedSeries, updatedSeries, true);
    }

    [Fact]
    public async Task UpdateSeries_UpdatesSeries_WhenTracked()
    {
        // Arrange
        var groupToSeed = new ScanlationGroupEntity()
        {
            Id = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
            Name = GroupNameConstants.AsuraScans,
            PublicName = "Asura Scans",
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var seriesToSeed = new SeriesEntity()
        {
            Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            ScanlationGroupId = groupToSeed.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.Add(seriesToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        var seriesToUpdate = new SeriesDto()
        {
            Id = seriesToSeed.Id,
            ScanlationGroupId = seriesToSeed.ScanlationGroupId,
            ExternalId = "series-1-updated",
            Title = "Series 1 Updated",
            RelativeCoverUrl = "/series-1-updated.webp",
        };

        // Act
        _seriesRepository.UpdateSeries(seriesToUpdate with { Title = "Updated" });
        _seriesRepository.UpdateSeries(seriesToUpdate);
        await _seriesRepository.SaveChangesAsync();

        // Assert
        _dbContext.ChangeTracker.Clear();

        var updatedSeries = await _dbContext.Series.FirstAsync(
            series => series.Id == seriesToUpdate.Id,
            TestContext.Current.CancellationToken);

        var expectedUpdatedSeries = new SeriesEntity
        {
            Id = seriesToUpdate.Id,
            ScanlationGroupId = seriesToUpdate.ScanlationGroupId,
            ExternalId = seriesToUpdate.ExternalId,
            Title = seriesToUpdate.Title,
            RelativeCoverUrl = seriesToUpdate.RelativeCoverUrl,
        };

        Assert.Equivalent(expectedUpdatedSeries, updatedSeries, true);
    }

    [Fact]
    public async Task AddChapter_AddsChapter()
    {
        // Arrange
        var groupToSeed = new ScanlationGroupEntity()
        {
            Id = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
            Name = GroupNameConstants.AsuraScans,
            PublicName = "Asura Scans",
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var seriesToSeed = new SeriesEntity()
        {
            Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            ScanlationGroupId = groupToSeed.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.Add(seriesToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        var chapterToAdd = new ChapterDto()
        {
            Id = Guid.Parse("0197eb79-24d6-7063-a73d-f4df3b320f68"),
            SeriesId = seriesToSeed.Id,
            ExternalId = "1",
            Title = "Chapter 1",
            Number = 1,
            AddedAt = new DateTimeOffset(2025, 7, 8, 0, 0, 0, default),
        };

        // Act
        _seriesRepository.AddChapter(chapterToAdd);
        await _seriesRepository.SaveChangesAsync();

        // Assert
        _dbContext.ChangeTracker.Clear();

        var addedChapter = await _dbContext.Chapters.FirstAsync(
            chapter => chapter.Id == chapterToAdd.Id,
            TestContext.Current.CancellationToken);

        var expectedAddedChapter = new ChapterEntity()
        {
            Id = chapterToAdd.Id,
            SeriesId = chapterToAdd.SeriesId,
            ExternalId = chapterToAdd.ExternalId,
            Title = chapterToAdd.Title,
            Number = chapterToAdd.Number,
            AddedAt = chapterToAdd.AddedAt,
        };

        Assert.Equivalent(expectedAddedChapter, addedChapter, true);
    }

    [Theory]
    [MemberData(nameof(GetChaptersViolatingUniqueConstraint))]
    public async Task AddChapter_ThrowsException_WhenUniqueConstraintIsViolated(
        ChapterDto firstChapter,
        ChapterDto secondChapter)
    {
        // Arrange
        var groupToSeed = new ScanlationGroupEntity()
        {
            Id = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
            Name = GroupNameConstants.AsuraScans,
            PublicName = "Asura Scans",
            BaseWebsiteUrl = "https://asu.ra",
            BaseCoverUrl = "https://gg.asu.ra",
        };

        var seriesToSeed = new SeriesEntity()
        {
            Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            ScanlationGroupId = groupToSeed.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.Add(seriesToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        // Act
        _seriesRepository.AddChapter(firstChapter);
        _seriesRepository.AddChapter(secondChapter);
        var exception = await Record.ExceptionAsync(_seriesRepository.SaveChangesAsync);

        // Assert
        Assert.Contains(
            "duplicate key value violates unique constraint",
            exception!.InnerException!.Message);
    }
}
