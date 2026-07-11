namespace RS.SetupApp.Core;

public interface IRegistryService
{
    void RegisterInstallerEntries(ProductManifest product, PackageManifest package, InstalledStateManifest state);

    void RemoveInstallerEntries(ProductManifest product, UninstallPlan plan, bool removeFileAssociations, bool removeAutorun);
}
