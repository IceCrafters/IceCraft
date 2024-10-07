// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Artefacts;

using IceCraft.Api.Package;

public interface IArtefactManager
{
    [Obsolete("Use VerifyArtefactAsync(IArtefactDefinition, PackageMeta) instead.")]
    Task<bool> VerifyArtefactAsync(RemoteArtefact artefact, PackageMeta package);
    Task<bool> VerifyArtefactAsync(IArtefactDefinition artefact, PackageMeta package);

    [Obsolete("Use GetSafeArtefactPathAsync(IArtefactDefinition, PackageMeta) instead.")]
    Task<string?> GetSafeArtefactPathAsync(RemoteArtefact artefact, PackageMeta package);
    Task<string?> GetSafeArtefactPathAsync(IArtefactDefinition artefact, PackageMeta package);
    
    /// <summary>
    /// Gets the path where the specified artefact would live at.
    /// </summary>
    /// <param name="artefact">The artefact.</param>
    /// <returns>The path to the artefact. May not exist or matches its checksum; but if it does it will exist at this location.</returns>
    [Obsolete("Use GetArtefactPath(IArtefactDefinition, PackageMeta) instead.")]
    string GetArtefactPath(RemoteArtefact artefact, PackageMeta package);

    /// <summary>
    /// Gets the path where the specified artefact would live at.
    /// </summary>
    /// <param name="artefact">The artefact.</param>
    /// <returns>The non-volatile unique storage location for the specified artefact; otherwise, if the artefact is <see cref="VolatileArtefact"/>, returns <see langword="null"/>.</returns>
    string? GetArtefactPath(IArtefactDefinition artefact, PackageMeta package);
    
    [Obsolete("UseCreateArtefactFile(IArtefactDefinition, PackageMeta) instead.")]
    Stream CreateArtefact(RemoteArtefact artefact, PackageMeta package);
    Stream CreateArtefactFile(IArtefactDefinition artefact, PackageMeta package);

    void CleanArtefacts();
}