namespace IceCraft.Core.Archive.Artefacts;

using System.Diagnostics;
using IceCraft.Core.Archive.Checksums;

/// <summary>
/// Represents an artefact available over HTTP(S).
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed record RemoteArtefact : IEquatable<RemoteArtefact>
{
    public RemoteArtefact(Uri downloadUri, string? checksum, IChecksumValidator checksumValidator)
    {
        DownloadUri = downloadUri;
        Checksum = checksum;
        ChecksumValidator = checksumValidator;
    }

    public Uri DownloadUri { get; }
    public string? Checksum { get; }
    public IChecksumValidator ChecksumValidator { get; }

    private string GetDebuggerDisplay()
    {
        return $"RemoteArtefact{{DownloadUri={DownloadUri},Checksum={Checksum},Validator=(is {ChecksumValidator.GetType()})}}";
    }
}
