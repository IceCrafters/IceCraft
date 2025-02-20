// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using System.IO.Abstractions;
using System.Reflection;
using System.Text.Json;
using IceCraft.Extensions.CentralRepo.Api;
using Jint;
using Jint.Runtime.Interop;
using Microsoft.Extensions.DependencyInjection;

// Full Speed Astern!

public class MashiroRuntime
{
    private readonly IMashiroLifetimeFactory _lifetimeFactory;
    private readonly IFileSystem _fileSystem;

    public MashiroRuntime(IMashiroLifetimeFactory lifetimeFactory, IFileSystem fileSystem)
    {
        _lifetimeFactory = lifetimeFactory;
        _fileSystem = fileSystem;
    }
    
    public delegate void ExpandPackageDelegate(string artefactFile, string targetDir);

    public delegate void RemovePackageDelegate(string targetDir);
    
    public delegate void OnPreprocessDelegate(string tempDir, string to);

    public delegate void ConfigureDelegate(string installDir);
    
    public delegate void UnConfigureDelegate(string installDir);
    
    private static readonly JsonNamingPolicy CamelCase = JsonNamingPolicy.CamelCase;

    private static readonly TypeResolver JintTypeResolver = new()
    {
        MemberNameCreator = NameCreator
    };
    
    private static readonly Options JintOptions = new()
    {
        Interop =
        {
            TypeResolver = JintTypeResolver,
            ExceptionHandler = static ex => ex is IMashiroApiError
        }
    };

    private static IEnumerable<string> NameCreator(MemberInfo info)
    {
        if (info.MemberType is MemberTypes.Method or MemberTypes.Property)
        {
            yield return CamelCase.ConvertName(info.Name);
        }
        
        yield return info.Name;
    }

    public IMashiroStateLifetime CreateStateLifetime(string scriptCode, string? fileName)
    {
        var script = Engine.PrepareScript(scriptCode,
            fileName);

        var result = _lifetimeFactory.Create();
        result.State.SetScript(script);
        result.State.AddApis();

        return result;
    }
    
    public async Task<IMashiroStateLifetime> CreateStateLifetimeAsync(string scriptFile)
    {
        return CreateStateLifetime(await _fileSystem.File.ReadAllTextAsync(scriptFile), 
            _fileSystem.Path.GetFileNameWithoutExtension(scriptFile));
    }
    
    internal static Engine CreateJintEngine()
    {
        var engine = new Engine(JintOptions);
        engine.SetValue(MashiroMetaBuilder.JsName, TypeReference.CreateTypeReference<MashiroMetaBuilder>(engine));
        engine.SetValue("AssetHandle", TypeReference.CreateTypeReference<MashiroAssetHandle>(engine));
        engine.SetValue("MFormatError", TypeReference.CreateTypeReference<MashiroFormatError>(engine));

        engine.SetValue("semVer", MashiroGlobals.SemVer);
        engine.SetValue("author", MashiroGlobals.Author);
        engine.SetValue("semRange", MashiroGlobals.SemRange);
        engine.SetValue("semRangeAny", MashiroGlobals.SemRangeAny);
        engine.SetValue("semRangeExact", MashiroGlobals.SemRangeExact);
        engine.SetValue("semRangeAtLeast", MashiroGlobals.SemRangeAtLeast);

        return engine;
    }
}