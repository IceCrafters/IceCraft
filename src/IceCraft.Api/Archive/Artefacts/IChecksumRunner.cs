// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Artefacts;

public interface IChecksumRunner
{
    /// <summary>
    /// Validates the specified stream with the given artefact. The stream will be read to end.
    /// </summary>
    /// <param name="artefact">The artefact to verify. Callers should expect that implementations will only accept
    /// <see cref="HashedArtefact"/> and <see cref="VolatileArtefact"/>.</param>
    /// <param name="stream">The stream to verify with.</param>
    /// <returns><see langword="true"/> if the verification is successful; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NotSupportedException">The <see cref="IArtefactDefinition"/> implementation specified is not supported.</exception>
    Task<bool> ValidateAsync(IArtefactDefinition artefact, Stream stream);
}
