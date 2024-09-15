namespace IceCraft.Extensions.CentralRepo.Network;

using IceCraft.Api.Client;
using SharpCompress.Common;
using SharpCompress.Readers;

public class RemoteRepositoryManager
{
    private readonly IOutputAdapter _output;

    private readonly string _csrDataPath;

    public RemoteRepositoryManager(IFrontendApp frontendApp)
    {
        _output = frontendApp.Output;

        _csrDataPath = Path.Combine(frontendApp.DataBasePath, "csr");
        LocalCachedRepoPath = Path.Combine(_csrDataPath, "repository");
    }
    
    internal string LocalCachedRepoPath { get; }

    internal async Task InitializeCacheAsync()
    {
        string downloadedPath;
        await using (var archiveStream = GetArchiveStream())
        {
            downloadedPath = await DownloadArchiveTempAsync(archiveStream);
        }

        await using var stream = File.OpenRead(downloadedPath);
        using var reader = ReaderFactory.Open(stream);
        
        reader.WriteAllToDirectory(LocalCachedRepoPath, new ExtractionOptions
        {
            ExtractFullPath = true,
            Overwrite = true
        });
    }
    
    private async Task<string> DownloadArchiveTempAsync(Stream stream)
    {
        _output.Log("Downloading repository information...");
        
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        await using var objective = File.Create(tempPath);
        await stream.CopyToAsync(objective);

        return tempPath;
    }
    
    private static Stream GetArchiveStream()
    {
        #if DEBUG
        var envVar = Environment.GetEnvironmentVariable("ICECRAFT_DBG_CSR_ARCHIVE_PATH");
        if (envVar != null && File.Exists(envVar))
        {
            return File.OpenRead(envVar);
        }
        #endif
        
        throw new NotImplementedException();
    }
}