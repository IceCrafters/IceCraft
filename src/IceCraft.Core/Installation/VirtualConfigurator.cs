// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;

public class VirtualConfigurator : IPackageConfigurator
{
    public Task ConfigurePackageAsync(string installDir, PackageMeta meta)
    {
        return Task.CompletedTask;
    }

    public Task UnconfigurePackageAsync(string installDir, PackageMeta meta)
    {
        return Task.CompletedTask;
    }
}