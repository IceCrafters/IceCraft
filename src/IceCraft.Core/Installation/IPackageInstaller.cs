namespace IceCraft.Core.Installation;

public interface IPackageInstaller
{
    Task InstallPackageAsync(string artefactFile, string targetDir);
}
