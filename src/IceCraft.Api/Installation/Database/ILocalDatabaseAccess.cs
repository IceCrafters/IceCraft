namespace IceCraft.Api.Installation.Database;

/// <summary>
/// Defines a provider for <see cref="ILocalDatabaseMutator"/> and <see cref="ILocalDatabaseReadHandle"/>.
/// </summary>
public interface ILocalDatabaseAccess
{
    ILocalDatabaseMutator GetMutator();
    ILocalDatabaseReadHandle GetReadHandle();
}
