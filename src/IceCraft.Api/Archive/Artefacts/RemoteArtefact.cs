// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Artefacts;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents an artefact available over HTTP(S).
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
[Obsolete("Use HashedArtefact instead.")]
public readonly struct RemoteArtefact : IEquatable<RemoteArtefact>
{
    [SetsRequiredMembers]
    public RemoteArtefact(string checksumType, string checksum)
    {
        ChecksumType = checksumType;
        Checksum = checksum;
    }

    [Obsolete("Artefacts will be downloaded from mirrors.")]
    public Uri? DownloadUri { get; init; }
    
    public required string Checksum { get; init; }
    public required string ChecksumType { get; init; }

    public bool Equals(RemoteArtefact other)
    {
        return other.ChecksumType == ChecksumType
            && other.Checksum.Equals(Checksum, StringComparison.OrdinalIgnoreCase);
    }

    private string GetDebuggerDisplay()
    {
        return $"RemoteArtefact{{Checksum={Checksum},Validator=(is {ChecksumType.GetType()})}}";
    }

    public override bool Equals(object? obj)
    {
        return obj is RemoteArtefact artefact && Equals(artefact);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Checksum, ChecksumType);
    }

    public static bool operator ==(RemoteArtefact left, RemoteArtefact right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RemoteArtefact left, RemoteArtefact right)
    {
        return !(left == right);
    }
}
