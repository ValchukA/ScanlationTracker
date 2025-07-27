using OneOf;
using OneOf.Types;
using ScanlationTracker.Core.Services.Contracts;

namespace ScanlationTracker.Core.Services.Interfaces;

public interface ISeriesService
{
    public Task<OneOf<Guid, SeriesNotFoundError, DuplicateTrackingError>> CreateTrackingAsync(
        Guid seriesId,
        Guid userId);

    public Task<OneOf<Success, TrackingNotFoundError, UnauthorizedError>> DeleteTrackingAsync(
        Guid trackingId,
        Guid userId);
}
