namespace IceCraft.Core.Archive.Artefacts;

using IceCraft.Core.Archive.Packaging;

public interface IArtefactManager
{
    Task<bool> VerifyArtefactAsync(RemoteArtefact artefact, PackageMeta package);
    Task<string?> GetSafeArtefactPathAsync(RemoteArtefact artefact, PackageMeta package);
    
    /// <summary>
    /// Gets the path where the specified artefact would live at.
    /// </summary>
    /// <param name="artefact">The artefact.</param>
    /// <returns>The path to the artefact. May not exist or matches its checksum; but if it does it will exist at this location.</returns>
    string GetArtefactPath(RemoteArtefact artefact, PackageMeta package);
    
    Stream CreateArtefact(RemoteArtefact artefact, PackageMeta package);

    void CleanArtefacts();
}