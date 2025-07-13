using Microsoft.EntityFrameworkCore;
using ScanlationTracker.Core.Models;
using ScanlationTracker.Core.Repositories;

namespace ScanlationTracker.Infrastructure.Database.Repositories;

internal class SeriesEfRepository : ISeriesRepository
{
    private readonly ScanlationDbContext _dbContext;

    public SeriesEfRepository(ScanlationDbContext dbContext) => _dbContext = dbContext;

    public async Task<ScanlationGroup[]> GetAllGroupsAsync()
    {
        var groupEntities = await _dbContext.ScanlationGroups.AsNoTracking().ToArrayAsync();

        return [.. groupEntities.Select(group => group.ToModel())];
    }

    public async Task<Series?> GetSeriesByExternalIdAsync(Guid groupId, string seriesExternalId)
    {
        var seriesEntity = await _dbContext.Series
            .AsNoTracking()
            .FirstOrDefaultAsync(series => series.ScanlationGroupId == groupId
                && series.ExternalId == seriesExternalId);

        return seriesEntity?.ToModel();
    }

    public async Task<Series?> GetSeriesByTitleAsync(Guid groupId, string seriesTitle)
    {
        var seriesEntity = await _dbContext.Series
            .AsNoTracking()
            .FirstOrDefaultAsync(series => series.ScanlationGroupId == groupId
                && series.Title == seriesTitle);

        return seriesEntity?.ToModel();
    }

    public async Task<Chapter?> GetLatestChapterAsync(Guid seriesId)
    {
        var chapterEntity = await _dbContext.Chapters
            .AsNoTracking()
            .OrderBy(chapter => chapter.Number)
            .LastOrDefaultAsync(chapter => chapter.SeriesId == seriesId);

        return chapterEntity?.ToModel();
    }

    public void AddSeries(Series series) => _dbContext.Series.Add(series.ToEntity());

    public void UpdateSeries(Series series)
    {
        var trackedSeries = _dbContext.Series.Local.FindEntry(series.Id);

        if (trackedSeries is not null)
        {
            trackedSeries.CurrentValues.SetValues(series.ToEntity());

            return;
        }

        _dbContext.Series.Update(series.ToEntity());
    }

    public void AddChapter(Chapter chapter) => _dbContext.Chapters.Add(chapter.ToEntity());

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
