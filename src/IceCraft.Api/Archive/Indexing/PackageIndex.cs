namespace IceCraft.Api.Archive.Indexing;

using System.Collections.ObjectModel;
using IceCraft.Api.Package;

public sealed class PackageIndex : ReadOnlyDictionary<string, CachedPackageSeriesInfo>
{
    public PackageIndex(IDictionary<string, CachedPackageSeriesInfo> dictionary) : base(dictionary)
    {
    }

    public CachedPackageInfo GetPackageInfo(PackageMeta meta)
    {
        if (!TryGetValue(meta.Id, out var seriesInfo))
        {
            throw new ArgumentException($"Package series {meta.Id} is not in this index.", nameof(meta));
        }

        if (!seriesInfo.Versions.TryGetValue(meta.Version.ToString(), out var result))
        {
            throw new ArgumentException($"Version {meta.Version} is not present for  series {meta.Id} in this index.", nameof(meta));
        }

        return result;
    }
}
