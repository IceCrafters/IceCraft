namespace IceCraft.Core.Archive.Indexing;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using IceCraft.Core.Archive.Packaging;

public sealed class PackageIndex : ReadOnlyDictionary<string, PackageMeta>
{
    public PackageIndex(IDictionary<string, PackageMeta> dictionary) : base(dictionary)
    {
    }
}
