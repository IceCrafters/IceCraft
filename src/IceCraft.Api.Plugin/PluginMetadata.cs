namespace IceCraft.Api.Plugin;

public readonly record struct PluginMetadata
{
    /// <summary>
    /// Gets the unique identifier of this instance.
    /// </summary>
    public required string Identifier { get; init; }
    
    /// <summary>
    /// Gets the version of this instance.
    /// </summary>
    public required string Version { get; init; }
    
    /// <summary>
    /// Gets the user friendly name of this instance.
    /// </summary>
    public string? Name { get; init; }
}