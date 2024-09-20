// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Util;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Package;

public class PackageIndexBuilder
{
    private readonly Dictionary<string, CachedPackageSeriesInfo> _dictionary = new();

    public PackageIndexBuilder WithPackage(PackageMeta meta, RemoteArtefact artefact)
    {
        var pkgInfo = new CachedPackageInfo()
        {
            Artefact = artefact,
            Metadata = meta
        };
        
        if (!_dictionary.TryGetValue(meta.Id, out var seriesInfo))
        {
            seriesInfo = new CachedPackageSeriesInfo()
            {
                Name = meta.Id,
                Versions = new Dictionary<string, CachedPackageInfo>
                {
                    {
                        meta.Version.ToString(), pkgInfo
                    }
                }
            };
            
            _dictionary.Add(meta.Id, seriesInfo);
            return this;
        }

        seriesInfo.Versions[meta.Version.ToString()] = pkgInfo;
        return this;
    }

    public PackageIndex Build()
    {
        return new PackageIndex(_dictionary);
    }
}