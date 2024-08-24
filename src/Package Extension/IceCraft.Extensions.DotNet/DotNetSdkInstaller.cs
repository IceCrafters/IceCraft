namespace IceCraft.Extensions.DotNet;

using IceCraft.Core.Installation;
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;

public class DotNetSdkInstaller : IPackageInstaller
{
    public Task ExpandPackageAsync(string artefactFile, string targetDir)
    {
        using var archive = GZipArchive.Open(artefactFile);
        archive.Entries.First().WriteToDirectory(targetDir);
        return Task.CompletedTask;
    }

    public Task RemovePackageAsync(string targetDir)
    {
        Directory.Delete(targetDir, true);
        return Task.CompletedTask;
    }
}