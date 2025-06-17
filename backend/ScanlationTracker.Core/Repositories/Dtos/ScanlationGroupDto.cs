namespace ScanlationTracker.Core.Repositories.Dtos;

public record ScanlationGroupDto
{
    public required Guid Id { get; init; }

    public required ScanlationGroupName Name { get; init; }

    public required string PublicName { get; init; }

    public required string BaseWebsiteUrl { get; init; }

    public required string BaseCoverUrl { get; init; }
}
