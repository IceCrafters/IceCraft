namespace IceCraft.Core.Archive.Checksums;

using System.Diagnostics;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Configuration;
using IceCraft.Core.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class DependencyChecksumRunner : IChecksumRunner
{
    private readonly IServiceProvider _provider;
    private readonly ILogger _logger;
    private readonly IManagerConfiguration _config;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public DependencyChecksumRunner(IServiceProvider provider,
        IManagerConfiguration config,
        ILogger<DependencyChecksumRunner> logger)
    {
        _logger = logger;
        _config = config;
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
