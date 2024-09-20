// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Artefacts;

public interface IChecksumRunner
{
    Task<bool> ValidateLocal(RemoteArtefact artefact, string file);
    
    Task<bool> ValidateLocal(RemoteArtefact artefact, Stream stream);
}
