namespace IceCraft.Repositories.Adoptium.Models;

using System.Text.Json.Serialization;
using JetBrains.Annotations;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers,
    Reason = "JSON model")]
internal class AdoptiumBinaryAsset
{
    public required string Os { get; init; }
    public required string Architecture { get; init; }
    public required string ImageType { get; init; }

    [JsonPropertyName("c_lib")]
    public string? CLib { get; init; }

    public required string JvmImpl { get; init; }

    public AdoptiumPackageArtefact? Package { get; init; }

    public AdoptiumPackageArtefact? Installer { get; init; }

    public string? HeapSize { get; init; }

    public long DownloadCount { get; init; }

    public required DateTime UpdatedAt { get; init; }

    public string? ScmRef { get; init; }

    public required string Project { get; init; }
}
