namespace IceCraft.Core.Archive.Artefacts;

using System.Security.Cryptography;
using System.Text;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Platform;
using IceCraft.Core.Util;

public class ArtefactManager : IArtefactManager
{
    private readonly IChecksumRunner _checksumRunner;
    private readonly string _artefactDirectory;

    public ArtefactManager(IFrontendApp frontendApp, IChecksumRunner checksumRunner)
    {
        _checksumRunner = checksumRunner;
        _artefactDirectory = Path.Combine(frontendApp.DataBasePath, "artefacts");

        Directory.CreateDirectory(_artefactDirectory);
    }
    
    public async Task<bool> VerifyArtefactAsync(RemoteArtefact artefact, PackageMeta package)
    {
        return await GetSafeArtefactPathAsync(artefact, package) != null;
    }

    public async Task<string?> GetSafeArtefactPathAsync(RemoteArtefact artefact, PackageMeta package)
    {
        var fileName = GetArtefactPath(artefact, package);
        
        if (!File.Exists(fileName)
            || !await _checksumRunner.ValidateLocal(artefact, fileName))
        {
            return null;
        }
        
        return fileName;
    }

    public string GetArtefactPath(RemoteArtefact artefact, PackageMeta package)
    {
        var idString = $"{package.Id}-{artefact.ChecksumType}-{artefact.Checksum}";
        var strHash = Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(idString)));

        return Path.Combine(_artefactDirectory,
            strHash);
    }

    public Stream CreateArtefact(RemoteArtefact artefact, PackageMeta package)
    {
        var fileName = GetArtefactPath(artefact, package);
        return File.Create(fileName);
    }

    public void CleanArtefacts()
    {
        var files = Directory.GetFiles(_artefactDirectory);
        var now = DateTime.UtcNow;
        
        foreach (var file in files)
        {
            if (now - File.GetCreationTimeUtc(file) > TimeSpan.FromDays(7))
            {
                File.Delete(file);
            }
        }
    }
}