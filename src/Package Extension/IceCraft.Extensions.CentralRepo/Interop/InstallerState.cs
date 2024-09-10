namespace IceCraft.Extensions.CentralRepo.Interop;

using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Api;
using Mond;

public class InstallerState
{
    private readonly MondState _mondState;
    private readonly MondProgram _mondProgram;

    internal InstallerState(MondState mondState, MondProgram program)
    {
        _mondState = mondState;
        _mondProgram = program;
    }

    public void Init(PackageMeta packageMeta)
    {
        _mondState.Call("initialize");

        _mondState["PACKAGE_ID"] = packageMeta.Id;
        _mondState["PACKAGE_VERSION"] = ScriptConvert.FromSemVersion(packageMeta.Version);
    }

    public void Expand(string archive, string to)
    {
        _mondState.Call("expand", archive, to);
    }

    public void Build(string from, string to)
    {
        _mondState.Call("build", from, to);
    }

    public void SetUp(string installDir)
    {
        _mondState.Call("setup", installDir);
    }
}
