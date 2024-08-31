namespace IceCraft.Extensions.CentralRepo.Models;

public class RemoteSeriesEntry
{
    public required IList<RemoteVersionEntry> Versions { get; init; }
}