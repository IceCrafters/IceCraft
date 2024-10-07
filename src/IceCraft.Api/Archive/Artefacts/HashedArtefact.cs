// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Artefacts;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents an artefact that is validated using a hash checksum and is stored
/// in a non-volatile storage for future use.
/// </summary>
public readonly record struct HashedArtefact : IArtefactDefinition
{
    [SetsRequiredMembers]
    public HashedArtefact(string checksumType, string checksum)
    {
        ChecksumType = checksumType;
        Checksum = checksum;
    }
    
    public required string Checksum { get; init; }
    public required string ChecksumType { get; init; }
}
