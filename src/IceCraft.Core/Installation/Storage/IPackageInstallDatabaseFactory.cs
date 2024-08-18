namespace IceCraft.Core.Installation.Storage;

public interface IPackageInstallDatabaseFactory
{
    Task<IPackageInstallDatabase> GetAsync();
    Task SaveAsync(string filePath);
    Task SaveAsync();
}
