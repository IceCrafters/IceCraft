// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Platform;

using System.IO;
using System.Threading.Tasks;
using IceCraft.Api.Platform;

public class PosixExecutionScriptGenerator : IExecutionScriptGenerator
{
    public async Task WriteExecutionScriptAsync(ExecutableRegistrationEntry entry, string executablePath, Stream stream)
    {
        var writer = new StreamWriter(stream);
        await writer.WriteLineAsync("#!/bin/sh");
        await writer.WriteLineAsync("# Generated by IceCraft");
        
        if (entry.EnvironmentVariables != null)
        {
            foreach (var variable in entry.EnvironmentVariables)
            {
                await writer.WriteLineAsync($"export {variable.Key}='{variable.Value.Replace('\'', '"')}'");
            }
        }

        await writer.WriteLineAsync($"'{executablePath}' $@");
        await writer.FlushAsync();
    }
}
