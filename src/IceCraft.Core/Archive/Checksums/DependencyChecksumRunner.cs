namespace IceCraft.Core.Archive.Checksums;

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
            _logger.LogError("File does not exist or is not a file, unable to validate checksum");
            return false;
        }
        await using var stream = File.OpenRead(file);
        return await ValidateLocal(artefact, stream);
    }

    [Obsolete("Use ValidateLocal(RemoteArtefact, string) instead.")]
    public async Task<bool> ValidateLocal(ArtefactMirrorInfo artefact, string file)
    {
        if (artefact.Checksum == null || artefact.ChecksumType == null)
        {
            throw new NotSupportedException("Unsupported artefact mirror information (legacy information not provided).");
        }
        
        _logger.LogWarning("Unsupported call to ArtefactMirrorInfo hash validation");
        _logger.LogWarning("FROM: {StackTrace}", Environment.StackTrace);
        
        if (!File.Exists(file))
        {
            _logger.LogError("File does not exist or is not a file, unable to validate checksum");
            return false;
        }

        var validator = _provider.GetKeyedService<IChecksumValidator>(artefact.ChecksumType);
        if (validator == null)
        {
            _logger.LogError("Package with remote {} uses an unsupported checksum type: {}",
                artefact.DownloadUri,
                artefact.ChecksumType);
            _logger.LogWarning("IceCraft will be unable to validate the package.");

            return _config.DoesAllowUncertainHash;
        }

        byte[] checkCode;

        await _semaphore.WaitAsync();
        await using (var stream = File.OpenRead(file))
        {
            checkCode = await validator.GetChecksumBinaryAsync(stream);
        }
        _semaphore.Release();

        var fileChecksum = validator.GetChecksumString(checkCode);
        return validator.CompareChecksum(fileChecksum, artefact.Checksum);
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
        return validator.CompareChecksum(fileChecksum, artefact.Checksum);
    }
}
