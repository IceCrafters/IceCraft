// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Dependency;

/// <summary>
/// Defines a hook that is invoked whenever dependency reference evaluation fails.
/// </summary>
public interface IDependencyHook
{
    /// <summary>
    /// Called when neither local or remote packages satisfy a dependency reference.
    /// </summary>
    /// <param name="reference">The reference to evaluate.</param>
    /// <returns><see langword="true"/> if the dependency resolver should assume dependency satisified; otherwise, <see langword="false"/>.</returns>
    bool EvaluateSatisfactory(DependencyReference reference);
}
