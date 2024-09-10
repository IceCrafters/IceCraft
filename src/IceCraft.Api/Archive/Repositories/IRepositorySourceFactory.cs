namespace IceCraft.Api.Archive.Repositories;

public interface IRepositorySourceFactory
{
    IRepositorySource CreateRepositorySource(IServiceProvider provider, out string name);
}
