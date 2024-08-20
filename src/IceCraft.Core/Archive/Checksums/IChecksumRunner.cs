﻿namespace IceCraft.Core.Archive.Checksums;

using IceCraft.Core.Archive.Artefacts;

public interface IChecksumRunner
{
    Task<bool> ValidateLocal(RemoteArtefact artefact, string file);
    Task<bool> ValidateLocal(ArtefactMirrorInfo artefact, string file);
}
