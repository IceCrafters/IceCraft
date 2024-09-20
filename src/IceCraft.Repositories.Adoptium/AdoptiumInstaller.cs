// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Repositories.Adoptium;

using System.Threading.Tasks;
using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using SharpCompress.Archives;

public class AdoptiumInstaller : IPackageInstaller
{
    public Task ExpandPackageAsync(string artefactFile, string targetDir, PackageMeta meta)
    {
        var archive = ArchiveFactory.Open(artefactFile);
        var dir = archive.Entries.First(x => x.IsDirectory && x.Key?.StartsWith("jdk-") == true);
        
        dir.WriteToDirectory(targetDir);
        return Task.CompletedTask;
    }

    public Task RemovePackageAsync(string targetDir, PackageMeta meta)
    {
        Directory.Delete(targetDir, true);
        return Task.CompletedTask;
    }
}
