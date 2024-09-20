// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.DotNet;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using SharpCompress.Common;
using SharpCompress.Readers;

public class DotNetSdkInstaller : IPackageInstaller
{
    public Task ExpandPackageAsync(string artefactFile, string targetDir, PackageMeta meta)
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

    public Task RemovePackageAsync(string targetDir, PackageMeta meta)
    {
        Directory.Delete(targetDir, true);
        return Task.CompletedTask;
    }
}