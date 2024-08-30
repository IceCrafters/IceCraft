namespace IceCraft.Core.Archive.Packaging;

public sealed record PackageTranscript : IEquatable<PackageTranscript>
{
    public required IReadOnlyList<PackageAuthorInfo> Authors { get; init; }

    public string? Description { get; init; }

    public string? License { get; init; }

    public PackageAuthorInfo Maintainer { get; init; }

    public PackageAuthorInfo PluginMaintainer { get; init; }
}
