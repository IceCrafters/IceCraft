﻿// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Repositories;

public interface IRepository
{
    /// <summary>
    /// Gets the series with the specified name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The returned series; <see langword="null"/> if not found.</returns>
    [Obsolete("This method is hard to implement without sacrificing performance, and is never used.")]
    IPackageSeries? GetSeriesOrDefault(string name);

    /// <summary>
    /// Enumerate all series available for this repository.
    /// </summary>
    /// <returns>An enumerable for the series.</returns>
    IEnumerable<IPackageSeries> EnumerateSeries();
    
    int GetExpectedSeriesCount();
}
