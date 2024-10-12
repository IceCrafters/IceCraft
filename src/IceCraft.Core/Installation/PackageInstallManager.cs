// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation;

using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Database;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Microsoft.Extensions.DependencyInjection;
using Semver;

public class PackageInstallManager : IPackageInstallManager
{
    private readonly IFrontendApp _frontend;
    private readonly IServiceProvider _serviceProvider;

    private readonly string _packagesPath;

    public PackageInstallManager(IFrontendApp frontend,
        IServiceProvider serviceProvider)
    {
        _frontend = frontend;
        _serviceProvider = serviceProvider;

        _packagesPath = Path.Combine(frontend.DataBasePath, "packages");
        CreateDirectories();
    }

    private void CreateDirectories()
    {
        Directory.CreateDirectory(_packagesPath);
    }

    [Obsolete("Use setup agent instead.")]
    public Task BulkInstallAsync(IAsyncEnumerable<DueInstallTask> packages,
        int expectedCount)
    {
        throw new NotSupportedException();
    }

    [Obsolete("Use setup agent instead.")]
    public Task InstallAsync(PackageMeta meta, string artefactPath)
    {
        throw new NotSupportedException();
    }

    public string GetInstalledPackageDirectory(PackageMeta meta)
    {
        if (!IsInstalled(meta))
        {
            throw new ArgumentException("The specified package was not installed.", nameof(meta));
        }

        return GetPackageDirectory(meta);
    }

    private string GetPackageDirectory(PackageMeta meta)
    {
        var versionPart = meta.Unitary
            ? "unitary"
            : CleanPath(meta.Version.ToString());

        return Path.Combine(_packagesPath,
            CleanPath(meta.Id),
            versionPart);
    }

    private static string CleanPath(string path)
    {
        return path.Replace('.', '_');
    }

    [Obsolete("Use setup agent instead.")]
    public Task UninstallAsync(PackageMeta meta)
    {
        throw new NotSupportedException();
    }

    public bool IsInstalled(PackageMeta? meta)
    {
        if (meta == null)
        {
            return false;
        }

        var database = _serviceProvider.GetReadHandle();

        return database.ContainsPackage(meta);
    }

    public bool IsInstalled(string packageName)
    {
        var database = _serviceProvider.GetReadHandle();
        
        return database.ContainsPackage(packageName);
    }

    public bool IsInstalled(string packageName, string version)
    {
        var database = _serviceProvider.GetReadHandle();

        return database.ContainsPackage(packageName, version);
    }

    public bool IsInstalled(DependencyReference dependency)
    {
        var database = _serviceProvider.GetReadHandle();

        return database.ContainsPackage(dependency.PackageId)
               && database.EnumeratePackages().Any(x => dependency.VersionRange.Contains(x.Version));
    }

    public PackageMeta? GetLatestMetaOrDefault(string packageName)
    {
        var database = _serviceProvider.GetReadHandle();

        return database.GetLatestVersionOrDefault(packageName);
    }

    public PackageMeta? GetLatestMetaOrDefault(string packageName, bool traceVirtualProvider)
    {
        var database = _serviceProvider.GetReadHandle();

        var latest = database.GetLatestVersionEntryOrDefault(packageName);
        if (latest == null)
        {
            return null;
        }

        if (!traceVirtualProvider 
            || latest.State != InstallationState.Virtual
            || !latest.ProvidedBy.HasValue)
        {
            return latest.Metadata;
        }

        var providedBy = latest.ProvidedBy.Value;
        var provider = database.GetValueOrDefault(providedBy)
         ?? throw new InvalidOperationException($"Provider package {latest.ProvidedBy.Value.PackageId} ({latest.ProvidedBy.Value.PackageVersion}) is null.");

        return provider.Metadata;
    }

    public async Task<PackageMeta?> GetLatestMetaOrDefaultAsync(string packageName,
        CancellationToken cancellationToken = default)
    {
        var database = _serviceProvider.GetReadHandle();

        return await database.GetLatestVersionOrDefaultAsync(packageName,
            cancellationToken);
    }

    public PackageMeta GetMeta(string packageName, SemVersion version)
    {
        var reader = _serviceProvider.GetReadHandle();
        return reader[packageName, version.ToString()].Metadata;
    }

    public PackageMeta? GetMetaOrDefault(string packageName, SemVersion version)
    {
        var reader = _serviceProvider.GetReadHandle();

        return !reader.TryGetValue(packageName, version, out var result)
            ? null
            : result.Metadata;
    }

    public async Task RegisterVirtualPackageAsync(PackageMeta virtualMeta, PackageReference origin)
    {
        var database = _serviceProvider.GetMutator();

        database.Put(new InstalledPackageInfo
        {
            Metadata = virtualMeta,
            State = InstallationState.Virtual,
            ProvidedBy = origin
        });

        await database.MaintainAsync();
        await database.StoreAsync();
    }

    public async Task PutPackageAsync(InstalledPackageInfo info)
    {
        var database = _serviceProvider.GetMutator();
        database.Put(info);
        
        await database.StoreAsync();
    }

    public async Task UnregisterPackageAsync(PackageMeta meta)
    {
        var database = _serviceProvider.GetMutator();
        
        database.Remove(meta.Id, meta.Version);
        await database.MaintainAsync();
        await database.StoreAsync();
    }

    public void ImportEnvironment(PackageMeta meta)
    {
        ArgumentNullException.ThrowIfNull(meta);

        if (!IsInstalled(meta))
        {
            throw new ArgumentException("The package is not installed.", nameof(meta));
        }

        var installDir = GetInstalledPackageDirectory(meta);
        
        var configurator = _serviceProvider.GetKeyedService<IPackageConfigurator>(meta.PluginInfo.ConfiguratorRef);
        if (configurator == null)
        {
            throw new KnownException($"Package '{meta.Id}' ({meta.Version}) does not define a valid configurator.");
        }
        
        configurator.ExportEnvironment(installDir, meta);
    }

    public async Task<bool> CheckForConflictAsync(PackageMeta package)
    {
        if (package.ConflictsWith == null)
        {
            return true;
        }

        var database = _serviceProvider.GetReadHandle();
        var isConflictFree = true;

        // Check for virtual packages with the same name
        if (database.EnumerateEntries(package.Id).Any(x => x.State == InstallationState.Virtual))
        {
            isConflictFree = false;
            _frontend.Output.Warning("A virtual package with the same name of '{0}' was found", package.Id);
        }

        await Task.Run(() =>
        {
            foreach (var reference in package.ConflictsWith)
            {
                if (!database.ContainsPackage(reference.PackageId))
                {
                    continue;
                }

                // ReSharper disable once InvertIf
                if (database.EnumerateEntries(reference.PackageId)
                    .Any(x => reference.VersionRange.Contains(x.Metadata.Version)
                              && !(x is { State: InstallationState.Virtual, ProvidedBy: not null }
                                   && x.ProvidedBy.Value.DoesPointTo(package))))
                {
                    _frontend.Output.Warning("Package {0} ({1}) conflicts with {2} {3}",
                        package.Id,
                        package.Version,
                        reference.PackageId,
                        reference.VersionRange);

                    isConflictFree = false;
                    break;
                }
            }
        });

        return isConflictFree;
    }

    public string GetUnsafePackageDirectory(PackageMeta meta)
    {
        return GetPackageDirectory(meta);
    }
}
