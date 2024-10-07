// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Developer;

using System.Collections.Generic;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public class DummyPackage : IPackage
{
    private readonly DummyPackageSeries _series;
    private readonly SemVersion _version;
    private readonly DependencyCollection? _dependencyCollection;
    private readonly string _name;
    private readonly bool _unitary;

    public DummyPackage(string name, 
        DummyPackageSeries series, 
        SemVersion version, 
        DependencyCollection? dependencyCollection,
        bool unitary)
    {
        _name = name;
        _series = series;
        _version = version;
        _dependencyCollection = dependencyCollection;
        _unitary = unitary;
    }

    public IPackageSeries Series => _series;

    public IArtefactDefinition GetArtefact()
    {
        return new HashedArtefact
        {
            ChecksumType = "sha256",
            Checksum = "5e9a7996fe94d7be10595d7133748760bf8348198b71b7a50fd8affaa980ac61"
        };
    }

    public PackageMeta GetMeta()
    {
        return new PackageMeta()
        {
            Id = _name,
            PluginInfo = new PackagePluginInfo("dummy-test", "dummy-test"),
            ReleaseDate = DateTime.MinValue,
            Version = _version,
            Dependencies = _dependencyCollection,
            Unitary = _unitary
        };
    }

    public IEnumerable<ArtefactMirrorInfo> GetMirrors()
    {
        return
        [
            new ArtefactMirrorInfo
            {
                DownloadUri = new Uri("http://www.msftconnecttest.com/connecttest.txt"),
                Name = "main",
                IsOrigin = true
            }
        ];
    }
}