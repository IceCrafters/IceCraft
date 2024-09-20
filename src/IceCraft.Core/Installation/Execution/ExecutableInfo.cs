// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Execution;

using IceCraft.Api.Platform;

public sealed record ExecutableInfo
{
    public required IDictionary<string, ExecutableRegistrationEntry> Registrations { get; init; }
    public ExecutableRegistrationEntry? Current { get; set; }
}
