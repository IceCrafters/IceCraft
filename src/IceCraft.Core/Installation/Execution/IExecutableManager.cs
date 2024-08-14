namespace IceCraft.Core.Installation.Execution;

using IceCraft.Core.Archive.Packaging;

public interface IExecutableManager
{
    Task LinkExecutableAsync(PackageMeta meta, string linkName, string from);
    Task<bool> UnlinkExecutableAsync(string linkName);
}
