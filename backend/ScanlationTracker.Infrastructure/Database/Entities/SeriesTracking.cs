namespace ScanlationTracker.Infrastructure.Database.Entities;

public class SeriesTracking
{
    public required Guid Id { get; init; }

    public required Guid SeriesId { get; init; }

    public required Guid UserId { get; init; }

    public Series? Series { get; init; }

    public User? User { get; init; }
}
