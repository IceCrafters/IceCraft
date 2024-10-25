// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Archive.Checksums;

using System.Diagnostics;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Exceptions;
using Microsoft.Extensions.DependencyInjection;

public class DependencyChecksumRunner : IChecksumRunner
{
    private readonly IServiceProvider _provider;

    public DependencyChecksumRunner(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task<bool> ValidateAsync(IArtefactDefinition artefact, Stream stream)
    {
        if (artefact is HashedArtefact hashed)
        {
            return await VerifyHashedAsync(hashed, stream);
        }
        else if (artefact is VolatileArtefact)
        {
            return true;
        }
        else
        {
            throw new NotSupportedException("Artefact definition type not supported.");
        }
    }

    private async Task<bool> VerifyHashedAsync(HashedArtefact artefact, Stream stream)
    {
        var validator = _provider.GetKeyedService<IChecksumValidator>(artefact.ChecksumType)
         ?? throw new KnownException($"Validator {artefact.ChecksumType} not found.");

        var checkCode = await validator.GetChecksumBinaryAsync(stream);
        
        var fileChecksum = validator.GetChecksumString(checkCode);
        Debug.WriteLine(fileChecksum.Equals(artefact.Checksum, StringComparison.OrdinalIgnoreCase));

        return validator.CompareChecksum(fileChecksum, artefact.Checksum);
    }
}
