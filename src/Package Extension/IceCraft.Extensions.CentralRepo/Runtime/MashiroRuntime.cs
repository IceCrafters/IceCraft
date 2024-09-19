namespace IceCraft.Extensions.CentralRepo.Runtime;

using System.Reflection;
using System.Text.Json;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Api;
using Jint;
using Jint.Runtime.Interop;

// Full Speed Astern!

public class MashiroRuntime
{
    private readonly IServiceProvider _serviceProvider;

    public MashiroRuntime(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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
    
    internal static readonly Options JintOptions = new()
    {
        Interop =
        {
            TypeResolver = JintTypeResolver
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

    public MashiroState CreateState(string scriptCode, string? fileName)
    {
        var engine = CreateJintEngine();

        var script = Engine.PrepareScript(scriptCode,
            fileName);   

        var result = new MashiroState(_serviceProvider, engine, script, fileName);
        result.AddFunctions();

        return result;
    }
    
    public async Task<MashiroState> CreateStateAsync(string scriptFile)
    {
        return CreateState(await File.ReadAllTextAsync(scriptFile), 
            Path.GetFileNameWithoutExtension(scriptFile));
    }
    
    private static Engine CreateJintEngine()
    {
        var engine = new Engine(JintOptions);
        engine.SetValue(MashiroMetaBuilder.JsName, TypeReference.CreateTypeReference<MashiroMetaBuilder>(engine));
        engine.SetValue("SemVer", MashiroGlobals.SemVer);
        engine.SetValue("Author", MashiroGlobals.Author);

        return engine;
    }
}