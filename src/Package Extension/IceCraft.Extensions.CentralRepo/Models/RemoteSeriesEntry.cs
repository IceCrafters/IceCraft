namespace IceCraft.Extensions.CentralRepo.Models;

using IceCraft.Core.Archive.Packaging;
using JetBrains.Annotations;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class RemoteSeriesEntry
{
    public required IList<RemoteVersionEntry> Versions { get; init; }
    
    public PackageTranscript? Transcript { get; init; }
}