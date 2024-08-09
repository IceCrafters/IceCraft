namespace IceCraft.Core.Archive.Indexing;

using System.Collections.Generic;
using System.Collections.ObjectModel;

public sealed class PackageIndex : ReadOnlyDictionary<string, CachedPackageSeriesInfo>
{
    public PackageIndex(IDictionary<string, CachedPackageSeriesInfo> dictionary) : base(dictionary)
    {
    }
}
