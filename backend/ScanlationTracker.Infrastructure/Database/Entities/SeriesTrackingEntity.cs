using Microsoft.EntityFrameworkCore;

namespace ScanlationTracker.Infrastructure.Database.Entities;

[Index(nameof(SeriesId), nameof(UserId), IsUnique = true)]
public class SeriesTrackingEntity
{
    public required Guid Id { get; init; }

    public required Guid SeriesId { get; init; }

    public required Guid UserId { get; init; }

    public SeriesEntity? Series { get; init; }

    public UserEntity? User { get; init; }
}
