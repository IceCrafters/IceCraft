namespace IceCraft.Core.Archive.Artefacts;

public sealed record ArtefactMirrorInfo
{
    public required string Name { get; init; }

    public required Uri DownloadUri { get; init; }

    /// <summary>
    /// Gets a value indicating whether this mirror have questionable status and
    /// should only be available behind a <c>--use-questionable-mirrors</c> switch.
    /// </summary>
    public bool IsQuestionable { get; init; }

    /// <summary>
    /// Gets a value indicating whether this mirror is the official file server (that is, the official mirror
    /// that all other servers are ultimately its downstream), and should be available even if the
    /// <c>--official-only</c> switch.
    /// </summary>
    public bool IsOrigin { get; init; }

    public required string Checksum { get; init; }

    public required string ChecksumType { get; init; }
}
