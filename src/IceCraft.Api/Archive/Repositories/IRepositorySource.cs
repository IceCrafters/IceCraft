// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Repositories;

/// <summary>
/// Defines a supplier for a single repository.
/// </summary>
/// <remarks>
/// It is expected that sources only to initialize cache, request version information etc. when creating and regenerating
/// repositories. Doing initialization in constructors or in field initialization can cause caches to unnecessarily generate
/// and process even if the source is disabled, and can prevent <see cref="RefreshAsync"/> from performing its purpose: 
/// reload and regenerate package version cache.
/// </remarks>
public interface IRepositorySource
{
    /// <summary>
    /// Creates the repositories.
    /// </summary>
    /// <remarks>
    /// Repositories should cache their data and only regenerate on <see cref="RefreshAsync"/>, and
    /// when first initialization.
    /// </remarks>
    /// <returns>The information regarding the created repositories.</returns>
    IEnumerable<RepositoryInfo> CreateRepositories();

    /// <summary>
    /// Deletes all cached data, and regenerate everything at the next <see cref="CreateRepositoryAsync"/> call.
    /// </summary>
    Task RefreshAsync();
}
