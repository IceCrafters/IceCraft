// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Repositories.Adoptium;

using System.Threading.Tasks;
using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Api.Platform;

public class AdoptiumConfigurator : IPackageConfigurator
{
    private readonly IExecutableManager _executableManager;

    public AdoptiumConfigurator(IExecutableManager executableManager)
    {
        _executableManager = executableManager;
    }

    public async Task ConfigurePackageAsync(string installDir, PackageMeta meta)
    {
        await _executableManager.RegisterAsync(meta, "java", "bin/java");
        await _executableManager.RegisterAsync(meta, "javaw", "bin/javaw");
    }

    public async Task UnconfigurePackageAsync(string installDir, PackageMeta meta)
    {
        await _executableManager.UnregisterAsync(meta, "java");
        await _executableManager.UnregisterAsync(meta, "javaw");
    }
}
