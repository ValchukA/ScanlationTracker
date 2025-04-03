namespace ScanlationTracker.Infrastructure.Database.Entities;

public class User
{
    public required Guid Id { get; init; }

    public required string Username { get; init; }
}
