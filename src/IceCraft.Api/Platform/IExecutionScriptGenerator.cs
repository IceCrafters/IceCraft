// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Platform;

public interface IExecutionScriptGenerator
{
    Task WriteExecutionScriptAsync(ExecutableRegistrationEntry entry, string executablePath, Stream stream);
}
