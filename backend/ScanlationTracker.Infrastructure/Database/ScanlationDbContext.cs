using Microsoft.EntityFrameworkCore;
using ScanlationTracker.Infrastructure.Database.Entities;

namespace ScanlationTracker.Infrastructure.Database;

public class ScanlationDbContext(DbContextOptions<ScanlationDbContext> options) : DbContext(options)
{
    public DbSet<ChapterEntity> Chapters { get; private set; }

    public DbSet<ScanlationGroupEntity> ScanlationGroups { get; private set; }

    public DbSet<SeriesEntity> Series { get; private set; }

    public DbSet<SeriesTrackingEntity> SeriesTrackings { get; private set; }

    public DbSet<UserEntity> Users { get; private set; }
}
