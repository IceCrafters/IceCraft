namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Api.Package;
using Mond.Binding;

[MondClass("PackageMeta")]
public partial class ScrPackageMeta
{
    private readonly PackageMeta _meta;
    private readonly ScrVersion _scrVersion;

    internal ScrPackageMeta(PackageMeta packageMeta)
    {
        _meta = packageMeta;
        _scrVersion = new ScrVersion(packageMeta.Version);
    }

    [MondFunction]
    public string Id => _meta.Id;
    
    [MondFunction]
    public ScrVersion Version => _scrVersion;
}