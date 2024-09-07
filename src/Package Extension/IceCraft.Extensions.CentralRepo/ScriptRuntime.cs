namespace IceCraft.Extensions.CentralRepo;

using IceCraft.Extensions.CentralRepo.Api;
using IceCraft.Extensions.CentralRepo.Interop;
using Mond;
using Mond.Libraries;

public class ScriptRuntime
{
    internal static InstallerState CreateState(string scriptFilePath)
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

        return new InstallerState(state, program);
    }
}