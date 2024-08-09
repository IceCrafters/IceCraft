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

    public required Uri DownloadUri { get; init; }
    public string? Checksum { get; init; }
    public required string ChecksumType { get; init; }

    private string GetDebuggerDisplay()
    {
        return $"RemoteArtefact{{DownloadUri={DownloadUri},Checksum={Checksum},Validator=(is {ChecksumType.GetType()})}}";
    }
}
