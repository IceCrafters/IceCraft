// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Client;

public interface IConfigScope
{
    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to get value from.</param>
    /// <returns>The value, or <see langword="null"/> if not found.</returns>
    string? Get(string key);
    void Set(string key, string value);
    void Remove(string key);
}