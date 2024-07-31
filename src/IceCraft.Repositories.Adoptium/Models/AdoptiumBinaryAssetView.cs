namespace IceCraft.Repositories.Adoptium.Models;

internal class AdoptiumBinaryAssetView
{
    public AdoptiumBinaryAsset? Binary { get; init; }
    public required string ReleaseName { get; init; }
    public required Uri ReleaseLink { get; init; }
    public string? Vendor { get; init; }
    public AdoptiumReleaseVersion? Version { get; init; }
}
