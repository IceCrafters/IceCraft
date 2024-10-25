// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Archive.Artefacts;

using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Client;
using IceCraft.Api.Package;

public class ArtefactManager : IArtefactManager
{
    private readonly IChecksumRunner _checksumRunner;
    private readonly string _artefactDirectory;
    private readonly IFileSystem _fileSystem;

    public ArtefactManager(IFrontendApp frontendApp, 
        IChecksumRunner checksumRunner,
        IFileSystem fileSystem)
    {
        _checksumRunner = checksumRunner;
        _artefactDirectory = Path.Combine(frontendApp.DataBasePath, "artefacts");
        _fileSystem = fileSystem;

        _fileSystem.Directory.CreateDirectory(_artefactDirectory);
    }
    
    public void CleanArtefacts()
    {
        var files = _fileSystem.Directory.GetFiles(_artefactDirectory);
        var now = DateTime.UtcNow;
        
        foreach (var file in files)
        {
            if (now - _fileSystem.File.GetCreationTimeUtc(file) > TimeSpan.FromDays(7))
            {
                _fileSystem.File.Delete(file);
            }
        }
    }

    public async Task<bool> VerifyArtefactAsync(IArtefactDefinition artefact, PackageMeta package)
    {
        if (artefact is VolatileArtefact)
        {
            return false;
        }

        return await GetSafeArtefactPathAsync(artefact, package) != null;
    }

    public async Task<string?> GetSafeArtefactPathAsync(IArtefactDefinition artefact, PackageMeta package)
    {
        var fileName = GetArtefactPath(artefact, package);
        
        if (!_fileSystem.File.Exists(fileName)
            || !await _checksumRunner.ValidateAsync(artefact, fileName))
        {
            return null;
        }
        
        return fileName;
    }

    public string? GetArtefactPath(IArtefactDefinition artefact, PackageMeta package)
    {
        if (artefact is not HashedArtefact hashed)
        {
            return null;
        }

        var idString = $"{package.Id}-{hashed.ChecksumType}-{hashed.Checksum}";
        var strHash = Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(idString)));

        return _fileSystem.Path.Combine(_artefactDirectory,
            strHash);
    }

    public Stream CreateArtefactFile(IArtefactDefinition artefact, PackageMeta package)
    {
        if (artefact is VolatileArtefact)
        {
            throw new ArgumentException("Volatile artefacts are not supported.", nameof(artefact));
        }

        var fileName = GetArtefactPath(artefact, package);
        return _fileSystem.File.Create(fileName!);
    }
}