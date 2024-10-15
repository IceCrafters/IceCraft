// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Runtime;
using IceCraft.Extensions.CentralRepo.Runtime.Security;
using JetBrains.Annotations;

public class MashiroPackages : ContextApi, IMashiroPackagesApi
{
    private readonly Func<ILocalPackageImporter> _importerSupplier;
    private readonly MashiroState _state;
    private const ExecutionContextType ContextTypes = ExecutionContextType.Configuration
                                                      | ExecutionContextType.Installation;
    private readonly IPackageInstallManager _installManager;

    public MashiroPackages(ContextApiRoot parent, 
        Func<ILocalPackageImporter> importerSupplier, 
        MashiroState state, 
        IPackageInstallManager installManager) : base(ContextTypes, parent)
    {
        _importerSupplier = importerSupplier;
        _state = state;
        _installManager = installManager;
    }

    [PublicAPI]
    public PackageMeta? GetLatestInstalledPackage(string id)
    {
        EnsureContext();
        return _installManager.GetLatestMetaOrDefault(id);
    }

    [PublicAPI]
    public PackageMeta? GetLatestInstalledPackage(string id, bool traceVirtualProvider)
    {
        EnsureContext();

        return _installManager.GetLatestMetaOrDefault(id, traceVirtualProvider);
    }

    [PublicAPI]
    public void ImportEnvironment(PackageMeta package)
    {
        EnsureContext();
        var importer = _importerSupplier();

        importer.ImportEnvironment(package);
    }

    [PublicAPI]
    public Task RegisterVirtual(string id)
    {
        EnsureContext(ExecutionContextType.Configuration);

        _state.EnsureMetadata();
        var metadata = _state.GetPackageMeta()!;

        return _installManager.RegisterVirtualPackageAsync(metadata with
        {
            Id = id
        }, metadata.CreateReference());
    }

    [PublicAPI]
    public Task RegisterVirtual(PackageMeta package)
    {
        EnsureContext(ExecutionContextType.Configuration);

        _state.EnsureMetadata();
        return _installManager.RegisterVirtualPackageAsync(package, _state.GetPackageMeta()!.CreateReference());
    }
}