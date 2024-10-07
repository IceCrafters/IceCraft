// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Database;

using System.Diagnostics.CodeAnalysis;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public static class LocalDatabaseExtensions
{
    public static IEnumerable<PackageMeta> EnumeratePackages(this ILocalDatabaseReadHandle readHandle, string identifier)
    {
        return readHandle.EnumerateEntries(identifier).Select(x => x.Metadata);
    }
    
    public static bool TryGetValue(this ILocalDatabaseReadHandle readHandle, PackageReference reference, [NotNullWhen(true)] out InstalledPackageInfo? entry)
    {
        return readHandle.TryGetValue(reference.PackageId, reference.PackageVersion, out entry);    
    }

    public static PackageMeta? GetLatestVersionOrDefault(this ILocalDatabaseReadHandle readHandle, 
        string identifier)
    {
        return readHandle.EnumeratePackages(identifier)
            .MaxBy(x => x.Version, SemVersion.SortOrderComparer);
    }

    public static InstalledPackageInfo? GetLatestVersionEntryOrDefault(this ILocalDatabaseReadHandle readHandle, 
        string identifier)
    {
        return readHandle.EnumerateEntries(identifier)
            .MaxBy(x => x.Metadata.Version, SemVersion.SortOrderComparer);
    }
    
    public static async Task<PackageMeta?> GetLatestVersionOrDefaultAsync(this ILocalDatabaseReadHandle readHandle, 
        string identifier,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
            readHandle.EnumeratePackages(identifier)
                .AsParallel()
                .WithCancellation(cancellationToken)
                .MaxBy(x => x.Version, SemVersion.SortOrderComparer), cancellationToken);
    }
}