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

    public async Task<bool> ValidateLocal(RemoteArtefact artefact, string file)
    {
        if (!File.Exists(file))
        {
            return false;
        }
        await using var stream = File.OpenRead(file);
        return await ValidateLocal(artefact, stream);
    }

    public async Task<bool> ValidateLocal(RemoteArtefact artefact, Stream stream)
    {
        var validator = _provider.GetKeyedService<IChecksumValidator>(artefact.ChecksumType);
        if (validator == null)
        {
            throw new KnownException($"Validator {artefact.ChecksumType} not found.");
        }
        
        var checkCode = await validator.GetChecksumBinaryAsync(stream);
        
        var fileChecksum = validator.GetChecksumString(checkCode);
        Debug.WriteLine(fileChecksum.Equals(artefact.Checksum, StringComparison.OrdinalIgnoreCase));

        return validator.CompareChecksum(fileChecksum, artefact.Checksum);
    }
}
