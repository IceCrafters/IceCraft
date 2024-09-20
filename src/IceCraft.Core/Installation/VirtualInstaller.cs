// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation;

using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
using IceCraft.Api.Package;

public class VirtualInstaller : IPackageInstaller
{
    public Task ExpandPackageAsync(string artefactFile, string targetDir, PackageMeta package)
    {
        return Task.CompletedTask;
    }

    public Task RemovePackageAsync(string targetDir, PackageMeta package)
    {
        throw new KnownException("Cannot 'uninstall' virtual package.");
    }
}