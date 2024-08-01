namespace IceCraft.Core.Serialization;

using System.Text.Json.Serialization;
using IceCraft.Core.Archive.Packaging;

[JsonSerializable(typeof(Dictionary<string, PackageMeta>), TypeInfoPropertyName = "BasePackageIndex")]
[JsonSerializable(typeof(PackageMeta))]
[JsonSerializable(typeof(string))]
internal partial class IceCraftCoreContext : JsonSerializerContext
{
}
