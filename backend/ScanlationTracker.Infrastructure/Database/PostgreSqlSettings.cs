using System.ComponentModel.DataAnnotations;

namespace ScanlationTracker.Infrastructure.Database;

internal class PostgreSqlSettings
{
    public const string SectionKey = "PostgreSql";

    [Required]
    public required string ConnectionString { get; init; }
}
