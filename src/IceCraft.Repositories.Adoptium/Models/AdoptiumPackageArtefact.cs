namespace IceCraft.Repositories.Adoptium.Models;

internal class AdoptiumPackageArtefact
{
    public required string Name { get; init; }
    public required string Link { get; init; }
    public long Size { get; init; }
    public string? Checksum { get; init; }
    public Uri? ChecksumLink { get; init; }
    public Uri? SignatureLink { get; init; }
    public long DownloadCount { get; init; }
    public Uri? MetadataLink { get; init; }
}
