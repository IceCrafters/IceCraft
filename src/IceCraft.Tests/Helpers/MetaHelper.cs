// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests.Helpers;

using Bogus;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Package;
using Semver;

public static class MetaHelper
{
    private static readonly Faker<PackageTranscript> TranscriptFaker = new Faker<PackageTranscript>()
        .StrictMode(true)
        .RuleFor(x => x.Description, f => f.Lorem.Sentence())
        .RuleFor(x => x.Authors, f =>
        [
            new PackageAuthorInfo(f.Name.FullName(),
                f.Internet.Email())
        ])
        .RuleFor(x => x.License, f => f.Random.Replace("?????-?#?"))
        .RuleFor(x => x.Maintainer, f => new PackageAuthorInfo(f.Name.FullName(),
            f.Internet.Email()))
        .RuleFor(x => x.PluginMaintainer, f => new PackageAuthorInfo(f.Name.FullName(),
            f.Internet.Email()));
    public static PackageMeta CreateMeta(SemVersion version)
    {
        var faker = new Faker<PackageMeta>()
            .StrictMode(true)
            .RuleFor(x => x.Id, f => f.Random.String2(10))
            .RuleFor(x => x.Version, _ => version)
            .RuleFor(x => x.Dependencies, _ => null)
            .RuleFor(x => x.PluginInfo, _ => new PackagePluginInfo("test", "test"))
            .RuleFor(x => x.Unitary, f => f.Random.Bool())
            .RuleFor(x => x.ReleaseDate, f => f.Date.Past())
            .RuleFor(x => x.Transcript, TranscriptFaker.Generate())
            .RuleFor(x => x.ConflictsWith, [])
            .RuleFor(x => x.AdditionalMetadata, _ => null)
            .RuleFor(x => x.CustomData, _ => null);

        return faker.Generate();
    }

    public static CachedPackageInfo CreateCachedInfo(SemVersion version)
    {
        var faker = new Faker<CachedPackageInfo>()
            .StrictMode(true)
            .RuleFor(x => x.Artefact, _ => new VolatileArtefact())
            .RuleFor(x => x.Metadata, _ => CreateMeta(version))
            .RuleFor(x => x.Mirrors, f =>
            [
                new ArtefactMirrorInfo
                {
                    Name = f.Internet.DomainWord(),
                    DownloadUri = new Uri(f.Internet.Url()),
                    IsOrigin = true,
                    IsQuestionable = false
                }
            ])
            .RuleFor(x => x.BestMirror, _ => null);

        return faker.Generate();
    }
}