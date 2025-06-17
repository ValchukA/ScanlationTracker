using Microsoft.EntityFrameworkCore;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Repositories.Dtos;

namespace ScanlationTracker.Infrastructure.Database.Repositories;

internal class SeriesEfRepository : ISeriesRepository
{
    private readonly ScanlationDbContext _dbContext;

    public SeriesEfRepository(ScanlationDbContext dbContext) => _dbContext = dbContext;

    public async Task<ScanlationGroupDto[]> GetAllGroupsAsync()
    {
        var groupEntitties = await _dbContext.ScanlationGroups.AsNoTracking().ToArrayAsync();

        return [.. groupEntitties.Select(group => group.ToDto())];
    }

    public async Task<SeriesDto?> GetSeriesAsync(Guid groupId, string seriesExternalId)
    {
        var seriesEntity = await _dbContext.Series
            .AsNoTracking()
            .FirstOrDefaultAsync(series => series.ScanlationGroupId == groupId
                && series.ExternalId == seriesExternalId);

        return seriesEntity?.ToDto();
    }

    public async Task<ChapterDto> GetLatestChapterAsync(Guid seriesId)
    {
        var chapterEntity = await _dbContext.Chapters
            .AsNoTracking()
            .OrderBy(chapter => chapter.Number)
            .LastAsync(chapter => chapter.SeriesId == seriesId);

        return chapterEntity.ToDto();
    }

    public void AddSeries(SeriesDto series) => _dbContext.Series.Add(series.ToEntity());

    public void UpdateSeries(SeriesDto series)
    {
        var trackedSeries = _dbContext.Series.Local.FindEntry(series.Id);

        if (trackedSeries is not null)
        {
            trackedSeries.CurrentValues.SetValues(series.ToEntity());

            return;
        }

        _dbContext.Series.Update(series.ToEntity());
    }

    public void AddChapter(ChapterDto chapter) => _dbContext.Chapters.Add(chapter.ToEntity());

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
