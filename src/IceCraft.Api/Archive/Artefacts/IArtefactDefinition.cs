// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Artefacts;

using System.Text.Json.Serialization;

/// <summary>
/// Represents an artefact.
/// </summary>
[JsonPolymorphic]
[JsonDerivedType(typeof(VolatileArtefact), "volatile")]
[JsonDerivedType(typeof(HashedArtefact), "hashed")]
public interface IArtefactDefinition
{
}
