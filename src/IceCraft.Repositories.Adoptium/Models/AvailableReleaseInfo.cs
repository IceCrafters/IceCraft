﻿namespace IceCraft.Repositories.Adoptium.Models;

using JetBrains.Annotations;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers,
    Reason = "JSON model")]
internal class AvailableReleaseInfo
{
    /// <summary>
    /// Gets the versions for which Adoptium have produced a GA release.
    /// </summary>
    public required List<int> AvailableReleases { get; init; }

    /// <summary>
    /// Gets the LTS versions for which Adoptium have produced a GA release.
    /// </summary>
    public required List<int> AvailableLtsReleases { get; init; }

    public int MostRecentLts { get; set; }
    public int MostRecentFeatureRelease { get; set; }
    public int MostRecentFeatureVersion { get; set; }
    public int TipVersion { get; set; }
}
