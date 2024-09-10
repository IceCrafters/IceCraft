namespace IceCraft.Core.Installation;

using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
using IceCraft.Core.Util;

public class VirtualInstaller : IPackageInstaller
{
    public Task ExpandPackageAsync(string artefactFile, string targetDir)
    {
        return Task.CompletedTask;
    }

    public Task RemovePackageAsync(string targetDir)
    {
        throw new KnownException("Cannot 'uninstall' virtual package.");
    }
}