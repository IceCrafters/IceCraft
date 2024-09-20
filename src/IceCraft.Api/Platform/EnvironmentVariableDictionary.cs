// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Platform;

public sealed class EnvironmentVariableDictionary : Dictionary<string, string>
{
    public EnvironmentVariableDictionary()
    {
    }

    public EnvironmentVariableDictionary(IDictionary<string, string> dictionary) : base(dictionary)
    {
    }

    public EnvironmentVariableDictionary(IEnumerable<KeyValuePair<string, string>> collection) : base(collection)
    {
    }

    public EnvironmentVariableDictionary(int capacity) : base(capacity)
    {
    }
}
