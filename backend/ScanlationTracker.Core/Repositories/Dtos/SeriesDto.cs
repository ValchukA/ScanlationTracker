namespace ScanlationTracker.Core.Repositories.Dtos;

public record SeriesDto
{
    public required Guid Id { get; init; }

    public required Guid ScanlationGroupId { get; init; }

    public required string ExternalId { get; init; }

    public required string Title { get; init; }

    public required string RelativeCoverUrl { get; init; }
}
