// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Network;

using IceCraft.Api.Client;
using SharpCompress.Common;
using SharpCompress.Readers;

public class RemoteRepositoryManager
{
    private readonly IOutputAdapter _output;

    public RemoteRepositoryManager(IFrontendApp frontendApp)
    {
        _output = frontendApp.Output;

        var csrDataPath = Path.Combine(frontendApp.DataBasePath, "csr");
        LocalCachedRepoPath = Path.Combine(csrDataPath, "repository");
    }
    
    internal string LocalCachedRepoPath { get; }

    internal void CleanPrevious()
    {
        if (Directory.Exists(LocalCachedRepoPath))
        {
            Directory.Delete(LocalCachedRepoPath, true);   
        }
    }
    
    internal async Task InitializeCacheAsync()
    {
        if (Directory.Exists(LocalCachedRepoPath))
        {
            return;
        }

        Directory.CreateDirectory(LocalCachedRepoPath);
        
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