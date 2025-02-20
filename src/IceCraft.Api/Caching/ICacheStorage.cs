﻿// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Caching;

/// <summary>
/// Defines an abstracted cache interface for non-volatile storage of various metadata (for example, on the disk).
/// </summary>
/// <remarks>
/// <para>
/// Callers should not download and store software binary or source packages in caches. These are expected to be
/// handled by the package manager through artefacts system, which handles download, storing and lifetime of artefact
/// objects.
/// </para>
/// </remarks>
public interface ICacheStorage
{
    /// <summary>
    /// Creates a cache object. If the object exists, it will be overwritten.
    /// </summary>
    /// <param name="objectName">The name of the object to create.</param>
    /// <returns></returns>
    Stream CreateObject(string objectName);

    /// <summary>
    /// Open an object for reading.
    /// </summary>
    /// <param name="objectName">The object to open.</param>
    /// <returns>A read-only stream for reading the object.</returns>
    Stream OpenReadObject(string objectName);

    /// <summary>
    /// Determines whether the specified object exist.
    /// </summary>
    /// <param name="objectName">The object to check.</param>
    /// <returns><see langword="true"/> if the object exists; otherwise, <see langword="false"/>.</returns>
    bool DoesObjectExist(string objectName);

    /// <summary>
    /// Deletes the specified object.
    /// </summary>
    /// <param name="objectName">The name of the object to delete.</param>
    void DeleteObject(string objectName);

    /// <summary>
    /// Deletes all storage objects.
    /// </summary>
    void Clear();
}
