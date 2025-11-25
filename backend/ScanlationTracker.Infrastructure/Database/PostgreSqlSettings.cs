using System.ComponentModel.DataAnnotations;

namespace ScanlationTracker.Infrastructure.Database;

internal class PostgreSqlSettings
{
    public const string SectionKey = "PostgreSql";

    [Required]
    public required string Host { get; init; }

    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }

    [Required]
    public required string Database { get; init; }
}
