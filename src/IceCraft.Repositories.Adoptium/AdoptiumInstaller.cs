namespace IceCraft.Repositories.Adoptium;

using System.Threading.Tasks;
using IceCraft.Core.Installation;
using SharpCompress.Archives;

public class AdoptiumInstaller : IPackageInstaller
{
    public Task InstallPackageAsync(string artefactFile, string targetDir)
    {
        var archive = ArchiveFactory.Open(artefactFile);
        var dir = archive.Entries.First(x => x.IsDirectory && x.Key.StartsWith("jdk-"));
        
        dir.WriteToDirectory(targetDir);
        return Task.CompletedTask;
    }
}
