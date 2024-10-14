// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;

public class LocalPackageImporter : ILocalPackageImporter
{
    private readonly IPackageInstallManager _installManager;
    private readonly IPackageSetupLifetime _lifetime;

    public LocalPackageImporter(IPackageInstallManager installManager,
        IPackageSetupLifetime lifetime)
    {
        _installManager = installManager;
        _lifetime = lifetime;
    }

    public void ImportEnvironment(PackageMeta meta)
    {
        ArgumentNullException.ThrowIfNull(meta);

        if (!_installManager.IsInstalled(meta))
        {
            throw new ArgumentException("The package is not installed.", nameof(meta));
        }

        var installDir = _installManager.GetInstalledPackageDirectory(meta);

        var configurator = _lifetime.GetConfigurator(meta.PluginInfo.ConfiguratorRef);
        configurator.ExportEnvironment(installDir, meta);
    }
}
