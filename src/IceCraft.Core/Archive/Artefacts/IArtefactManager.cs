namespace IceCraft.Core.Archive.Artefacts;

public interface IArtefactManager
{
    Task<bool> VerifyArtefactAsync(RemoteArtefact artefact);
    Task<string?> GetSafeArtefactPathAsync(RemoteArtefact artefact);
    
    /// <summary>
    /// Gets the path where the specified artefact would live at.
    /// </summary>
    /// <param name="artefact">The artefact.</param>
    /// <returns>The path to the artefact. May not exist or matches its checksum; but if it does it will exist at this location.</returns>
    string GetArtefactPath(RemoteArtefact artefact);
    
    Stream CreateArtefact(RemoteArtefact artefact);

    void CleanArtefacts();
}