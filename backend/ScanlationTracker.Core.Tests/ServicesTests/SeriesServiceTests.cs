using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OneOf.Types;
using ScanlationTracker.Core.Models;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Repositories.Exceptions;
using ScanlationTracker.Core.Services;
using ScanlationTracker.Core.Services.Contracts;
using Xunit;

namespace ScanlationTracker.Core.Tests.ServicesTests;

public class SeriesServiceTests
{
    [Fact]
    public async Task CreateTrackingAsync_CreatesTrackingAndReturnsTrackingId()
    {
        // Arrange
        var seriesId = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1");
        var userId = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86");

        var seriesRepository = Substitute.For<ISeriesRepository>();
        var seriesService = new SeriesService(seriesRepository);

        // Act
        var trackingId = (await seriesService.CreateTrackingAsync(seriesId, userId)).AsT0;

        // Assert
        Assert.NotEqual(Guid.Empty, trackingId);

        seriesRepository
            .Received()
            .AddTracking(Arg.Is<SeriesTracking>(tracking =>
                tracking.Id == trackingId
                && tracking.SeriesId == seriesId
                && tracking.UserId == userId));
        await seriesRepository
            .Received()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CreateTrackingAsync_ReturnsSeriesNotFoundError_WhenSeriesDoesNotExist()
    {
        // Arrange
        var userId = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86");

        var seriesRepository = Substitute.For<ISeriesRepository>();

        seriesRepository
            .SaveChangesAsync()
            .ThrowsAsync(new ForeignKeyConstraintException(new Exception()));

        var seriesService = new SeriesService(seriesRepository);

        // Act
        var result = await seriesService.CreateTrackingAsync(Guid.Empty, userId);

        // Assert
        Assert.IsType<SeriesNotFoundError>(result.Value);
    }

    [Fact]
    public async Task CreateTrackingAsync_ReturnsDuplicateTrackingError_WhenTrackingAlreadyExists()
    {
        // Arrange
        var seriesId = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1");
        var userId = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86");

        var seriesRepository = Substitute.For<ISeriesRepository>();

        seriesRepository
            .SaveChangesAsync()
            .ThrowsAsync(new UniqueConstraintException(new Exception()));

        var seriesService = new SeriesService(seriesRepository);

        // Act
        var result = await seriesService.CreateTrackingAsync(seriesId, userId);

        // Assert
        Assert.IsType<DuplicateTrackingError>(result.Value);
    }

    [Fact]
    public async Task DeleteTrackingAsync_DeletesTrackingAndReturnsSuccess()
    {
        // Arrange
        var savedTracking = new SeriesTracking
        {
            Id = Guid.Parse("019842a1-5a7a-7feb-944b-59ee7f4c7222"),
            SeriesId = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            UserId = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86"),
        };

        var seriesRepository = Substitute.For<ISeriesRepository>();

        seriesRepository
            .GetTrackingAsync(savedTracking.Id)
            .Returns(savedTracking);
        seriesRepository
            .SaveChangesAsync()
            .Returns(1);

        var seriesService = new SeriesService(seriesRepository);

        // Act
        var result = await seriesService.DeleteTrackingAsync(
            savedTracking.Id,
            savedTracking.UserId);

        // Assert
        Assert.IsType<Success>(result.Value);

        seriesRepository
            .Received()
            .DeleteTracking(savedTracking.Id);
        await seriesRepository
            .Received()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteTrackingAsync_ReturnsTrackingNotFoundError_WhenTrackingDoesNotExist()
    {
        // Arrange
        var userId = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86");

        var seriesRepository = Substitute.For<ISeriesRepository>();
        var seriesService = new SeriesService(seriesRepository);

        // Act
        var result = await seriesService.DeleteTrackingAsync(Guid.Empty, userId);

        // Assert
        Assert.IsType<TrackingNotFoundError>(result.Value);
    }

    [Fact]
    public async Task DeleteTrackingAsync_ReturnsTrackingNotFoundError_WhenTrackingDeletedConcurrently()
    {
        // Arrange
        var savedTracking = new SeriesTracking
        {
            Id = Guid.Parse("019842a1-5a7a-7feb-944b-59ee7f4c7222"),
            SeriesId = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            UserId = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86"),
        };

        var seriesRepository = Substitute.For<ISeriesRepository>();

        seriesRepository
            .GetTrackingAsync(savedTracking.Id)
            .Returns(savedTracking);

        var seriesService = new SeriesService(seriesRepository);

        // Act
        var result = await seriesService.DeleteTrackingAsync(
           savedTracking.Id,
           savedTracking.UserId);

        // Assert
        Assert.IsType<TrackingNotFoundError>(result.Value);
    }

    [Fact]
    public async Task DeleteTrackingAsync_ReturnsUnauthorizedError_WhenTrackingCreatedByAnotherUser()
    {
        // Arrange
        var userId = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86");
        var savedTracking = new SeriesTracking
        {
            Id = Guid.Parse("019842a1-5a7a-7feb-944b-59ee7f4c7222"),
            SeriesId = Guid.Parse("0197e6d9-8e74-7112-81fe-d6256a6c9fb1"),
            UserId = Guid.Parse("01984c7a-937a-7724-8112-af1bcceab32e"),
        };

        var seriesRepository = Substitute.For<ISeriesRepository>();

        seriesRepository
            .GetTrackingAsync(savedTracking.Id)
            .Returns(savedTracking);

        var seriesService = new SeriesService(seriesRepository);

        // Act
        var result = await seriesService.DeleteTrackingAsync(savedTracking.Id, userId);

        // Assert
        Assert.IsType<UnauthorizedError>(result.Value);
    }
}
