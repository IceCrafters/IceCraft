// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Api.Platform;
using IceCraft.Extensions.CentralRepo.Runtime;
using IceCraft.Extensions.CentralRepo.Runtime.Security;

public class MashiroBinary : ContextApi, IMashiroBinaryApi
{
    private readonly IExecutableManager _executableManager;
    private readonly IMashiroMetaTransfer _metaTransfer;
    
    public MashiroBinary(ContextApiRoot parent, 
        IExecutableManager executableManager, 
        IMashiroMetaTransfer metaTransfer) : base(ExecutionContextType.Configuration, parent)
    {
        _executableManager = executableManager;
        _metaTransfer = metaTransfer;
    }

    public async Task Register(string fileName, string path)
    {
        await _executableManager.RegisterAsync(_metaTransfer.PackageMeta!,
            fileName,
            path);
    }
    
    public async Task Register(string fileName, string path, EnvironmentVariableDictionary envVars)
    {
        await _executableManager.RegisterAsync(_metaTransfer.PackageMeta!,
            fileName,
            path,
            envVars);
    }

    public async Task Unregister(string fileName)
    {
        await _executableManager.UnregisterAsync(_metaTransfer.PackageMeta!,
            fileName);
    }
}