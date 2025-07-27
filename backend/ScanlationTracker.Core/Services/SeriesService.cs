using OneOf;
using OneOf.Types;
using ScanlationTracker.Core.Models;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Repositories.Exceptions;
using ScanlationTracker.Core.Services.Contracts;
using ScanlationTracker.Core.Services.Interfaces;

namespace ScanlationTracker.Core.Services;

internal class SeriesService : ISeriesService
{
    private readonly ISeriesRepository _seriesRepository;

    public SeriesService(ISeriesRepository seriesRepository) => _seriesRepository = seriesRepository;

    public async Task<OneOf<Guid, SeriesNotFoundError, DuplicateTrackingError>> CreateTrackingAsync(
        Guid seriesId,
        Guid userId)
    {
        var tracking = new SeriesTracking
        {
            Id = Guid.CreateVersion7(),
            SeriesId = seriesId,
            UserId = userId,
        };

        _seriesRepository.AddTracking(tracking);

        try
        {
            await _seriesRepository.SaveChangesAsync();
        }
        catch (ForeignKeyConstraintException)
        {
            return default(SeriesNotFoundError);
        }
        catch (UniqueConstraintException)
        {
            return default(DuplicateTrackingError);
        }

        return tracking.Id;
    }

    public async Task<OneOf<Success, TrackingNotFoundError, UnauthorizedError>> DeleteTrackingAsync(
        Guid trackingId,
        Guid userId)
    {
        var tracking = await _seriesRepository.GetTrackingAsync(trackingId);

        if (tracking is null)
        {
            return default(TrackingNotFoundError);
        }

        if (tracking.UserId != userId)
        {
            return default(UnauthorizedError);
        }

        _seriesRepository.DeleteTracking(trackingId);
        var deletedCount = await _seriesRepository.SaveChangesAsync();

        return deletedCount != 0 ? default(Success) : default(TrackingNotFoundError);
    }
}
