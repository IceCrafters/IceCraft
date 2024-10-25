// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Artefacts;

using IceCraft.Api.Package;

public interface IArtefactManager
{
    Task<bool> VerifyArtefactAsync(IArtefactDefinition artefact, PackageMeta package);
    Task<string?> GetSafeArtefactPathAsync(IArtefactDefinition artefact, PackageMeta package);
    
    /// <summary>
    /// Gets the path where the specified artefact would live at.
    /// </summary>
    /// <param name="artefact">The artefact.</param>
    /// <returns>The non-volatile unique storage location for the specified artefact; otherwise, if the artefact is <see cref="VolatileArtefact"/>, returns <see langword="null"/>.</returns>
    string? GetArtefactPath(IArtefactDefinition artefact, PackageMeta package);

    /// <summary>
    /// Creates an empty file in the non-volatile storage location of the atrefact, and opens
    /// it for write access.
    /// </summary>
    /// <param name="artefact">The artefact to create file for. Must not be <see cref="VolatileArtefact"/>.</param>
    /// <param name="package">The package to create file for.</param>
    /// <returns>The stream opened for the artefact to store at.</returns>
    /// <exception cref="ArgumentException">The artefact specified is <see cref="VolatileArtefact"/>.</exception>
    Stream CreateArtefactFile(IArtefactDefinition artefact, PackageMeta package);

    void CleanArtefacts();
}