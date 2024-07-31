namespace IceCraft.Core.Configuration;

/// <summary>
/// Defines statically-typed configuration for the package manager.
/// </summary>
public interface IManagerConfiguration
{
    bool IsSourceEnabled(string sourceId);
}
