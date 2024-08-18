namespace IceCraft.Repositories.Adoptium;

using System.Threading.Tasks;
using IceCraft.Core.Installation;
using SharpCompress.Archives;

public class AdoptiumInstaller : IPackageInstaller
{
    public Task ExpandPackageAsync(string artefactFile, string targetDir)
    {
        var archive = ArchiveFactory.Open(artefactFile);
        var dir = archive.Entries.First(x => x.IsDirectory && x.Key?.StartsWith("jdk-") == true);
        
        dir.WriteToDirectory(targetDir);
        return Task.CompletedTask;
    }

    public Task RemovePackageAsync(string targetDir)
    {
        Directory.Delete(targetDir, true);
        return Task.CompletedTask;
    }
}
