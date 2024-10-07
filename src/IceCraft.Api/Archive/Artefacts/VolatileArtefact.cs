// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Artefacts;

/// <summary>
/// Represents an atrefact that will be downloaded to a temporary location and will
/// not be hash checked.
/// </summary>
public readonly record struct VolatileArtefact : IArtefactDefinition
{
}
