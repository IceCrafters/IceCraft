namespace IceCraft.Extensions.CentralRepo;

using IceCraft.Extensions.CentralRepo.Api;
using Mond;
using Mond.Libraries;

public class ScriptRuntime
{
    internal static MondState CreateState(string scriptFilePath)
    {
        var state = new MondState
        {
            Libraries = new MondLibraryManager
            {
                new ScrVersion.Library(),
                new ScrPackageMeta.Library()
            }
        };

        var program = MondProgram.FromFile(scriptFilePath);
        state.Load(program);

        return state;
    }
}