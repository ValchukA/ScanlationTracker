namespace ScanlationTracker.Core.Repositories;

public interface ISeriesRepositoryFactory
{
    public ISeriesRepository CreateRepository();
}
