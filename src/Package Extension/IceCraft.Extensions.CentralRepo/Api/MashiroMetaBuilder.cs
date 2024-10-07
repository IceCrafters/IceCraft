// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public class MashiroMetaBuilder
{
    public const string JsName = "MetaBuilder";
    private readonly string _id;
    
    private SemVersion? _version;
    private PackageAuthorInfo[] _authors = [];
    private PackageAuthorInfo? _maintainer;
    private PackageAuthorInfo? _pluginMaintainer;
    private string? _license;
    private string? _description;
    private DateTime? _releaseDate;
    private bool _unitary;

    private readonly DependencyCollection _dependencies = [];

    private MashiroMetaBuilder(string id)
    {
        _id = id;
    }

    public static MashiroMetaBuilder Id(string id)
    {
        return new MashiroMetaBuilder(id);
    }

    public MashiroMetaBuilder Version(SemVersion version)
    {
        _version = version;
        return this;
    }

    public MashiroMetaBuilder Authors(params PackageAuthorInfo[] authors)
    {
        _authors = authors;
        return this;
    }

    public MashiroMetaBuilder Maintainer(PackageAuthorInfo maintainer)
    {
        _maintainer = maintainer;
        return this;
    }

    public MashiroMetaBuilder PluginMaintainer(PackageAuthorInfo pluginMaintainer)
    {
        _pluginMaintainer = pluginMaintainer;
        return this;
    }

    public MashiroMetaBuilder License(string license)
    {
        _license = license;
        return this;
    }

    public MashiroMetaBuilder Date(DateTime releaseDate)
    {
        _releaseDate = releaseDate;
        return this;
    }

    public MashiroMetaBuilder Description(string description)
    {
        _description = description;
        return this;
    }

    public MashiroMetaBuilder Dependency(string packageId, SemVersionRange versionRange)
    {
        _dependencies.Add(new DependencyReference(packageId, versionRange));
        return this;
    }

    public MashiroMetaBuilder Unitary()
    {
        _unitary = true;
        return this;
    }

    public PackageMeta Build()
    {
        if (_version == null)
        {
            throw new InvalidOperationException("Version not set");
        }

        if (!_releaseDate.HasValue)
        {
            throw new InvalidOperationException("Release date not set");
        }
        
        var result = new PackageMeta
        {
            Id = _id,
            Version = _version,
            ReleaseDate = _releaseDate.Value,
            PluginInfo = new PackagePluginInfo("mashiro", "mashiro"),
            Transcript = new PackageTranscript
            {
                Authors = _authors,
                Description = _description,
                License = _license,
                Maintainer = _maintainer ?? default,
                PluginMaintainer = _pluginMaintainer ?? default,
            },
            Dependencies = _dependencies,
            Unitary = _unitary
        };

        return result;
    }
}