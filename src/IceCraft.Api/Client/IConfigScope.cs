// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Client;

public interface IConfigScope
{
    string? Get(string key);
    void Set(string key, string value);
    void Remove(string key);
}