// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Client;

/// <summary>
/// Defines statically-typed configuration for the package manager.
/// </summary>
public interface IManagerConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether artefacts without a checksum
    /// are allowed to be installed.
    /// </summary>
    bool DoesAllowUncertainHash { get; set; }

    bool IsSourceEnabled(string sourceId);
    void SetSourceEnabled(string sourceId, bool enabled);

    string GetCachePath();
}
