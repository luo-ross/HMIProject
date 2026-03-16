namespace RS.SetupApp.Core;

public interface IRegistryService
{
    void RegisterInstallerEntries(ProductManifest product, PackageManifest package, InstalledStateManifest state);

    void RemoveInstallerEntries(ProductManifest product, InstalledStateManifest state, bool removeFileAssociations, bool removeAutorun);
}
