namespace IceCraft.Repositories.Adoptium.Models;

using JetBrains.Annotations;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers,
    Reason = "JSON model")]
internal class AdoptiumBinaryAssetView
{
    public AdoptiumBinaryAsset? Binary { get; init; }
    public required string ReleaseName { get; init; }
    public required Uri ReleaseLink { get; init; }
    public string? Vendor { get; init; }
    public AdoptiumReleaseVersion? Version { get; init; }
}

