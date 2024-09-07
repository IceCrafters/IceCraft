namespace IceCraft.Core.Installation.Execution;

using System.Collections.Generic;

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
