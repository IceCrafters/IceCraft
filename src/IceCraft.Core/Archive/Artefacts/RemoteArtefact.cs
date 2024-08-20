namespace IceCraft.Core.Archive.Artefacts;

using System.Diagnostics;

/// <summary>
/// Represents an artefact available over HTTP(S).
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed record RemoteArtefact
{
    public RemoteArtefact()
    {
    }

    [Obsolete("Artefacts will be downloaded from mirrors.")]
    public Uri? DownloadUri { get; init; }
    
    public required string Checksum { get; init; }
    public required string ChecksumType { get; init; }

    private string GetDebuggerDisplay()
    {
        return $"RemoteArtefact{{Checksum={Checksum},Validator=(is {ChecksumType.GetType()})}}";
    }
}
