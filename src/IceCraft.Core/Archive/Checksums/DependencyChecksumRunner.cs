namespace IceCraft.Core;

using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class DependencyChecksumRunner : IChecksumRunner
{
    private readonly IServiceProvider _provider;
    private readonly ILogger _logger;
    private readonly IManagerConfiguration _config;

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

        if (artefact.Checksum == null)
        {
            _logger.LogError("Package with remote {} does not provide checksum.", artefact.DownloadUri);
            _logger.LogWarning("IceCraft will be unable to validate the package.");

            return _config.DoesAllowUncertainHash;
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

        using (var stream = File.OpenRead(file))
        {
            checkCode = await validator.GetChecksumBinaryAsync(stream);
        }

        var fileChecksum = validator.GetChecksumString(checkCode);
        return validator.CompareChecksum(fileChecksum, artefact.Checksum);
    }
}
