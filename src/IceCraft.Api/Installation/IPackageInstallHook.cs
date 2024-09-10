namespace IceCraft.Api.Installation;

using IceCraft.Api.Package;

public interface IPackageInstallHook
{
    void OnBeforePackageExpand(PackageMeta meta, string expandTo, string installPath);
    void OnBeforePackageConfigure(PackageMeta meta, string installPath);
    void OnPackageInstalled(PackageMeta meta, string installPath);
}
