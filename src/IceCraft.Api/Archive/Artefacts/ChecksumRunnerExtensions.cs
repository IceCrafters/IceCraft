// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Artefacts;

public static class ChecksumRunnerExtensions
{
    /// <summary>
    /// Vaildates the specified file with the given artefact.
    /// </summary>
    /// <param name="runner">The runner to use for verification.</param>
    /// <param name="artefact">The artefact to verify. Callers should expect that implementations will only accept
    /// <see cref="HashedArtefact"/> and <see cref="VolatileArtefact"/>.</param>
    /// <param name="file">The file to verify with.</param>
    /// <returns><see langword="true"/> if the verification is successful; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NotSupportedException">The <see cref="IArtefactDefinition"/> implementation specified is not supported.</exception>
    public static async Task<bool> ValidateAsync(this IChecksumRunner runner, IArtefactDefinition artefact, string file)
    {
        await using var stream = File.OpenRead(file);
        return await runner.ValidateAsync(artefact, stream);
    }
}
