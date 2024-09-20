// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Developer;

using System.Threading.Tasks;
using IceCraft.Api.Installation;
using IceCraft.Api.Package;

public class DummyPackageConfigurator : IPackageConfigurator
{
    public async Task ConfigurePackageAsync(string installDir, PackageMeta meta)
    {
        await File.WriteAllTextAsync(Path.Combine(installDir, "configured"), "Dummy Configuration Package");
    }

    public Task UnconfigurePackageAsync(string installDir, PackageMeta meta)
    {
        File.Delete(Path.Combine(installDir, "configured"));
        return Task.CompletedTask;
    }
}
