namespace IceCraft.Extensions.CentralRepo.Runtime;

using System.Reflection;
using System.Text.Json;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Api;
using Jint;
using Jint.Runtime.Interop;

// Full Speed Astern!

public static class MashiroRuntime
{
    public delegate Task ExpandPackageAsync(string artefactFile, string targetDir, PackageMeta meta);

    public delegate Task RemovePackageAsync(string targetDir, PackageMeta meta);
    
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
        yield return CamelCase.ConvertName(info.Name);
    }

    public static MashiroState CreateState(string scriptFile)
    {
        var engine = CreateJintEngine();
        var script = Engine.PrepareScript(scriptFile);

        var result = new MashiroState(engine, script);
        result.AddFunctions();

        return result;
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