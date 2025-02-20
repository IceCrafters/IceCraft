// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using IceCraft.Api.Package;

public interface IArtefactPreprocessor
{
    /// <summary>
    /// Preprocesses the expanded artefact then copies the preprocessed artefact to the target
    /// directory.
    /// </summary>
    /// <param name="tempExpandDir">The location where the artefact is temporaily expanded at.</param>
    /// <param name="installDir">The location where the artefact will be installed at.</param>
    /// <param name="meta">The metadata information of the package that is being installed.</param>
    Task Preprocess(string tempExpandDir, string installDir, PackageMeta meta);
}
