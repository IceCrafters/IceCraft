namespace IceCraft.Api.Archive.Artefacts;

public interface IChecksumRunner
{
    Task<bool> ValidateLocal(RemoteArtefact artefact, string file);
    
    Task<bool> ValidateLocal(RemoteArtefact artefact, Stream stream);
}
