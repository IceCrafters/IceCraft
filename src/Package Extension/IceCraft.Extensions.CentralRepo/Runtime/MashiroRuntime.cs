namespace IceCraft.Extensions.CentralRepo.Runtime;

using System.Reflection;
using System.Text.Json;
using IceCraft.Extensions.CentralRepo.Api;
using Jint;
using Jint.Runtime.Interop;

// Full Speed Astern!

public static class MashiroRuntime
{
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

    public static Engine CreateJintEngine()
    {
        var engine = new Engine(JintOptions);
        engine.SetValue(MashiroMetaBuilder.JsName, TypeReference.CreateTypeReference<MashiroMetaBuilder>(engine));
        engine.SetValue("SemVer", MashiroGlobals.SemVer);
        engine.SetValue("Author", MashiroGlobals.Author);

        return engine;
    }
}