// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Developer;

using System.Threading.Tasks;
using IceCraft.Api.Installation;
using IceCraft.Api.Package;

public class DummyPackageInstaller : IPackageInstaller
{
    public Task ExpandPackageAsync(string artefactFile, string targetDir, PackageMeta meta)
    {
        Directory.CreateDirectory(targetDir);
        File.Move(artefactFile, Path.Combine(targetDir, "connecttest.txt"), true);
        return Task.CompletedTask;
    }

    public Task RemovePackageAsync(string targetDir, PackageMeta meta)
    {
        File.Delete(Path.Combine(targetDir, "connecttest.txt"));
        return Task.CompletedTask;
    }
}
