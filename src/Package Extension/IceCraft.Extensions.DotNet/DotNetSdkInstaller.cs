namespace IceCraft.Extensions.DotNet;

using IceCraft.Api.Installation;
using IceCraft.Core.Installation;
using SharpCompress.Common;
using SharpCompress.Readers;

public class DotNetSdkInstaller : IPackageInstaller
{
    public Task ExpandPackageAsync(string artefactFile, string targetDir)
    {
        using var stream = File.OpenRead(artefactFile);
        using var reader = ReaderFactory.Open(stream);
        
        reader.WriteAllToDirectory(targetDir, new ExtractionOptions()
        {
            ExtractFullPath = true,
            Overwrite = true
        });
        return Task.CompletedTask;
    }

    public Task RemovePackageAsync(string targetDir)
    {
        Directory.Delete(targetDir, true);
        return Task.CompletedTask;
    }
}