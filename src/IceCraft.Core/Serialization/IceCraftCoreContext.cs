namespace IceCraft.Core.Serialization;

using System.Text.Json.Serialization;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Execution;
using IceCraft.Core.Installation.Storage;

[JsonSerializable(typeof(Dictionary<string, PackageMeta>), TypeInfoPropertyName = "BasePackageIndex")]
[JsonSerializable(typeof(PackageMeta))]
[JsonSerializable(typeof(RemoteArtefact))]
[JsonSerializable(typeof(CachedPackageInfo))]
[JsonSerializable(typeof(CachedPackageSeriesInfo))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Dictionary<string, CachedPackageSeriesInfo>), TypeInfoPropertyName = "BasePackageIndex_v0_1")]
[JsonSerializable(typeof(ArtefactMirrorInfo))]
[JsonSerializable(typeof(IEnumerable<ArtefactMirrorInfo>))]
[JsonSerializable(typeof(PackageInstallDatabaseFactory.ValueMap), TypeInfoPropertyName = "PackageInstallValueMap")]
[JsonSerializable(typeof(Dictionary<string, ExecutableEntry>), TypeInfoPropertyName = "ExecutableDataFile")]
internal partial class IceCraftCoreContext : JsonSerializerContext
{
}
