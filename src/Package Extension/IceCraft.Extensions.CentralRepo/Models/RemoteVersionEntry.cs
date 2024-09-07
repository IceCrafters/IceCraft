namespace IceCraft.Extensions.CentralRepo.Models;

using System.Text.Json.Serialization;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Serialization;
using Semver;

public sealed class RemoteVersionEntry
{
    public PackageTranscript? Transcript { get; init; }
    
    [JsonConverter(typeof(SemVersionConverter))]
    public required SemVersion Version { get; init; }
    
    public required RemoteArtefact Artefact { get; init; }
    
    
    public required IList<ArtefactMirrorInfo> Mirrors { get; init; }
    
    public required DateTime ReleaseDate { get; init; }
    
    public bool Unitary { get; init; }
    
    public DependencyCollection? Dependencies { get; init; }
    public DependencyCollection? ConflictsWith { get; init; }

    public required Uri InstallerScript { get; init; }
}