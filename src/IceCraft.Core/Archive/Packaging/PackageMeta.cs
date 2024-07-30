namespace IceCraft.Core.Archive.Packaging;

public class PackageMeta
{
    public PackageMeta(string id, string version, DateTime releaseDate)
    {
        Id = id;
        Version = version;
        ReleaseDate = releaseDate;
    }

    public string Id { get; }

    public string Version { get; }

    public DateTime ReleaseDate { get; }
}
