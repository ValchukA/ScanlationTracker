using Microsoft.EntityFrameworkCore;
using ScanlationTracker.Core;
using ScanlationTracker.Core.Models;
using ScanlationTracker.Core.Repositories.Exceptions;
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
        var expectedGroups = new ScanlationGroup[]
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

        Assert.Equivalent(expectedGroups, groups, true);
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
        var expectedSeries = new Series
        {
            Id = seriesToSeed.Id,
            ScanlationGroupId = seriesToSeed.ScanlationGroupId,
            ExternalId = seriesToSeed.ExternalId,
            Title = seriesToSeed.Title,
            RelativeCoverUrl = seriesToSeed.RelativeCoverUrl,
        };

        Assert.Equivalent(expectedSeries, series, true);
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

        var seriesToSeed = new SeriesEntity[]
        {
            new()
            {
                Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
                ScanlationGroupId = groupToSeed.Id,
                ExternalId = "series-2",
                Title = "Series",
                RelativeCoverUrl = "/series-2.webp",
            },
            new()
            {
                Id = Guid.Parse("0197ebbb-682b-7a08-aa52-cad3d0a9294e"),
                ScanlationGroupId = groupToSeed.Id,
                ExternalId = "series-1",
                Title = "Series",
                RelativeCoverUrl = "/series-1.webp",
            },
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.AddRange(seriesToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        // Act
        var series = await _seriesRepository.GetSeriesByTitleAsync(
            groupToSeed.Id,
            seriesToSeed[0].Title);

        // Assert
        var expectedSeries = new Series[]
        {
            new()
            {
                Id = seriesToSeed[0].Id,
                ScanlationGroupId = seriesToSeed[0].ScanlationGroupId,
                ExternalId = seriesToSeed[0].ExternalId,
                Title = seriesToSeed[0].Title,
                RelativeCoverUrl = seriesToSeed[0].RelativeCoverUrl,
            },
            new()
            {
                Id = seriesToSeed[1].Id,
                ScanlationGroupId = seriesToSeed[1].ScanlationGroupId,
                ExternalId = seriesToSeed[1].ExternalId,
                Title = seriesToSeed[1].Title,
                RelativeCoverUrl = seriesToSeed[1].RelativeCoverUrl,
            },
        };

        Assert.Equivalent(expectedSeries, series, true);
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
        var expectedLatestChapter = new Chapter
        {
            Id = chaptersToSeed[0].Id,
            SeriesId = chaptersToSeed[0].SeriesId,
            ExternalId = chaptersToSeed[0].ExternalId,
            Title = chaptersToSeed[0].Title,
            Number = chaptersToSeed[0].Number,
            AddedAt = chaptersToSeed[0].AddedAt,
        };

        Assert.Equivalent(expectedLatestChapter, latestChapter, true);
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
    public async Task GetTrackingAsync_ReturnsTracking()
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

        var userToSeed = new UserEntity
        {
            Id = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86"),
            Username = "andva",
        };

        var trackingToSeed = new SeriesTrackingEntity
        {
            Id = Guid.Parse("019842a1-5a7a-7feb-944b-59ee7f4c7222"),
            SeriesId = seriesToSeed.Id,
            UserId = userToSeed.Id,
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.Add(seriesToSeed);
        _dbContext.Users.Add(userToSeed);
        _dbContext.SeriesTrackings.Add(trackingToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        // Act
        var tracking = await _seriesRepository.GetTrackingAsync(trackingToSeed.Id);

        // Assert
        var expectedTracking = new SeriesTracking
        {
            Id = trackingToSeed.Id,
            SeriesId = trackingToSeed.SeriesId,
            UserId = trackingToSeed.UserId,
        };

        Assert.Equivalent(expectedTracking, tracking, true);
    }

    [Fact]
    public async Task GetTrackingAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var tracking = await _seriesRepository.GetTrackingAsync(Guid.Empty);

        // Assert
        Assert.Null(tracking);
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

        var seriesToAdd = new Series()
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

        var chapterToAdd = new Chapter()
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

    [Fact]
    public async Task AddTracking_AddsTracking()
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

        var userToSeed = new UserEntity
        {
            Id = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86"),
            Username = "andva",
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.Add(seriesToSeed);
        _dbContext.Users.Add(userToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        var trackingToAdd = new SeriesTracking
        {
            Id = Guid.Parse("019842a1-5a7a-7feb-944b-59ee7f4c7222"),
            SeriesId = seriesToSeed.Id,
            UserId = userToSeed.Id,
        };

        // Act
        _seriesRepository.AddTracking(trackingToAdd);
        await _seriesRepository.SaveChangesAsync();

        // Assert
        _dbContext.ChangeTracker.Clear();

        var addedTracking = await _dbContext.SeriesTrackings.FirstAsync(
            tracking => tracking.Id == trackingToAdd.Id,
            TestContext.Current.CancellationToken);

        var expectedAddedTracking = new SeriesTrackingEntity()
        {
            Id = trackingToAdd.Id,
            SeriesId = trackingToAdd.SeriesId,
            UserId = trackingToAdd.UserId,
        };

        Assert.Equivalent(expectedAddedTracking, addedTracking, true);
    }

    [Fact]
    public async Task UpdateSeries_UpdatesSeries()
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

        var seriesToUpdate = new Series()
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
    public async Task DeleteTracking_DeletesTracking()
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

        var userToSeed = new UserEntity
        {
            Id = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86"),
            Username = "andva",
        };

        var trackingToSeed = new SeriesTrackingEntity
        {
            Id = Guid.Parse("019842a1-5a7a-7feb-944b-59ee7f4c7222"),
            SeriesId = seriesToSeed.Id,
            UserId = userToSeed.Id,
        };

        _dbContext.ScanlationGroups.Add(groupToSeed);
        _dbContext.Series.Add(seriesToSeed);
        _dbContext.Users.Add(userToSeed);
        _dbContext.SeriesTrackings.Add(trackingToSeed);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        // Act
        _seriesRepository.DeleteTracking(trackingToSeed.Id);
        await _seriesRepository.SaveChangesAsync();

        // Assert
        _dbContext.ChangeTracker.Clear();

        var trackingExists = await _dbContext.SeriesTrackings.AnyAsync(
            tracking => tracking.Id == trackingToSeed.Id,
            TestContext.Current.CancellationToken);

        Assert.False(trackingExists);
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnsAffectedRowsCount()
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

        var seriesToAdd = new Series()
        {
            Id = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            ScanlationGroupId = groupToSeed.Id,
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        // Act
        _seriesRepository.AddSeries(seriesToAdd);
        var affectedRowsCount = await _seriesRepository.SaveChangesAsync();

        // Assert
        Assert.Equal(1, affectedRowsCount);
    }

    [Fact]
    public async Task SaveChangesAsync_ThrowsUniqueConstraintException_WhenUniqueConstraintIsViolated()
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

        var seriesToAdd = new Series[]
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
        var action = _seriesRepository.SaveChangesAsync;

        // Assert
        await Assert.ThrowsAsync<UniqueConstraintException>(action);
    }

    [Fact]
    public async Task SaveChangesAsync_ThrowsForeignKeyConstraintException_WhenForeignKeyConstraintIsViolated()
    {
        // Arrange
        var seriesToAdd = new Series
        {
            Id = Guid.Parse("0197ebbb-682b-7a08-aa52-cad3d0a9294e"),
            ScanlationGroupId = Guid.Parse("0197e151-db58-7431-8844-ba45db8d16de"),
            ExternalId = "series-1",
            Title = "Series 1",
            RelativeCoverUrl = "/series-1.webp",
        };

        // Act
        _seriesRepository.AddSeries(seriesToAdd);
        var action = _seriesRepository.SaveChangesAsync;

        // Assert
        await Assert.ThrowsAsync<ForeignKeyConstraintException>(action);
    }
}
