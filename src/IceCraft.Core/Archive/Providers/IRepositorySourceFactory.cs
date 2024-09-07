namespace IceCraft.Core.Archive.Providers;

public interface IRepositorySourceFactory
{
    IRepositorySource CreateRepositorySource(IServiceProvider provider, out string name);
}
