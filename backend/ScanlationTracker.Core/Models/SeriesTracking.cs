namespace ScanlationTracker.Core.Models;

public class SeriesTracking
{
    public required Guid Id { get; init; }

    public required Guid SeriesId { get; init; }

    public required Guid UserId { get; init; }
}
