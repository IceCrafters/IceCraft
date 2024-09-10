namespace IceCraft.Api.Installation;

public interface IPackageInstallDatabaseFactory
{
    Task<IPackageInstallDatabase> GetAsync();
    Task SaveAsync(string filePath);
    Task SaveAsync();
    Task MaintainAndSaveAsync();
}
