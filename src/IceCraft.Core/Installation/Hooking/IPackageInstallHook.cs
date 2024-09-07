namespace IceCraft.Core.Installation.Hooking;

using IceCraft.Core.Archive.Packaging;

public interface IPackageInstallHook
{
    void OnBeforePackageExpand(PackageMeta meta, string expandTo, string installPath);
    void OnBeforePackageConfigure(PackageMeta meta, string installPath);
    void OnPackageInstalled(PackageMeta meta, string installPath);
}
