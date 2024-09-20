namespace IceCraft.Core.Installation;

using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
using IceCraft.Api.Package;

public class VirtualInstaller : IPackageInstaller
{
    public Task ExpandPackageAsync(string artefactFile, string targetDir, PackageMeta package)
    {
        return Task.CompletedTask;
    }

    public Task RemovePackageAsync(string targetDir, PackageMeta package)
    {
        throw new KnownException("Cannot 'uninstall' virtual package.");
    }
}