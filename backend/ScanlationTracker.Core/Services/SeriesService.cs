using ScanlationTracker.Core.Repositories;

namespace ScanlationTracker.Core.Services;

internal class SeriesService : ISeriesService
{
    private readonly ISeriesRepository _seriesRepository;

    public SeriesService(ISeriesRepository seriesRepository) => _seriesRepository = seriesRepository;

    public async Task UpdateSeriesAsync()
    {
        var groups = await _seriesRepository.GetAllGroupsAsync();

        foreach (var group in groups)
        {
            Console.WriteLine(group.PublicName);
        }
    }
}
