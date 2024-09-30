// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Runtime.Security;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

public class MashiroPackages : ContextApi
{
    private readonly IServiceProvider _serviceProvider;

    private const ExecutionContextType ContextTypes = ExecutionContextType.Configuration
                                                      | ExecutionContextType.Installation;
    
    public MashiroPackages(ContextApiRoot parent, IServiceProvider serviceProvider) : base(ContextTypes, parent)
    {
        _serviceProvider = serviceProvider;
    }

    [PublicAPI]
    public PackageMeta? GetLatestInstalledPackage(string id)
    {
        EnsureContext();
        var installManager = _serviceProvider.GetRequiredService<IPackageInstallManager>();

        return installManager.GetLatestMetaOrDefault(id);
    }

    [PublicAPI]
    public void ImportEnvironment(PackageMeta package)
    {
        EnsureContext();
        var installManager = _serviceProvider.GetRequiredService<IPackageInstallManager>();

        installManager.ImportEnvironment(package);
    }
}