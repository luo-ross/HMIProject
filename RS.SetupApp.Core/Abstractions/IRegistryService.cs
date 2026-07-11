namespace RS.SetupApp.Core;

public interface IRegistryService
{
    string CaptureInstallerEntriesSnapshot(ProductManifest product, InstalledStateManifest state);

    string CaptureInstallerEntriesSnapshot(ProductManifest product, UninstallPlan plan);

    void RestoreInstallerEntriesSnapshot(string snapshot);

    void DeleteValue(InstallScope scope, string keyPath, string valueName);

    void RegisterInstallerEntries(ProductManifest product, PackageManifest package, InstalledStateManifest state);

    void RemoveInstallerEntries(ProductManifest product, UninstallPlan plan, bool removeFileAssociations, bool removeAutorun);
}
