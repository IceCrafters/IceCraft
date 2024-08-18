namespace IceCraft.Core.Installation;

public interface IPackageInstaller
{
    Task ExpandPackageAsync(string artefactFile, string targetDir);
    Task RemovePackageAsync(string targetDir);
}
