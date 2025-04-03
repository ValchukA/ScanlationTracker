using Microsoft.EntityFrameworkCore;
using ScanlationTracker.Infrastructure.Database.Entities;

namespace ScanlationTracker.Infrastructure.Database;

public class ScanlationDbContext(DbContextOptions<ScanlationDbContext> options) : DbContext(options)
{
    public DbSet<Chapter> Chapters { get; private set; }

    public DbSet<ScanlationGroup> ScanlationGroups { get; private set; }

    public DbSet<Series> Series { get; private set; }

    public DbSet<SeriesTracking> SeriesTrackings { get; private set; }

    public DbSet<User> Users { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Chapter>()
            .HasIndex(chapter => new { chapter.SeriesId, chapter.ExternalId })
            .IsUnique();

        modelBuilder
            .Entity<ScanlationGroup>()
            .HasIndex(scanlationGroup => scanlationGroup.Name)
            .IsUnique();

        modelBuilder
            .Entity<ScanlationGroup>()
            .Property(scanlationGroup => scanlationGroup.Name)
            .HasConversion<string>();

        modelBuilder
            .Entity<Series>()
            .HasIndex(series => new { series.ScanlationGroupId, series.ExternalId })
            .IsUnique();

        modelBuilder
            .Entity<SeriesTracking>()
            .HasIndex(seriesTracking => new { seriesTracking.SeriesId, seriesTracking.UserId })
            .IsUnique();

        modelBuilder
            .Entity<User>()
            .HasIndex(user => user.Username)
            .IsUnique();
    }
}
