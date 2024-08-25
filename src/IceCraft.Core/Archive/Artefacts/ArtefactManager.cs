namespace IceCraft.Core.Archive.Artefacts;

using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Platform;

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
    
    public async Task<bool> VerifyArtefactAsync(RemoteArtefact artefact)
    {
        return await GetSafeArtefactPathAsync(artefact) != null;
    }

    public async Task<string?> GetSafeArtefactPathAsync(RemoteArtefact artefact)
    {
        var fileName = GetArtefactPath(artefact);
        
        if (!File.Exists(fileName)
            || !await _checksumRunner.ValidateLocal(artefact, fileName))
        {
            return null;
        }
        
        return fileName;
    }

    public string GetArtefactPath(RemoteArtefact artefact)
    {
        return Path.Combine(_artefactDirectory,
            $"{artefact.ChecksumType}-{artefact.Checksum}");
    }

    public Stream CreateArtefact(RemoteArtefact artefact)
    {
        var fileName = GetArtefactPath(artefact);
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