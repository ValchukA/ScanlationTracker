using Microsoft.EntityFrameworkCore;

namespace ScanlationTracker.Infrastructure.Database.Entities;

[Index(nameof(Username), IsUnique = true)]
public class UserEntity
{
    public required Guid Id { get; init; }

    public required string Username { get; init; }
}
