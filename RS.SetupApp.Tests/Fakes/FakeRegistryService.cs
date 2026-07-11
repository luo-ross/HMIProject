using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Fakes;

public sealed class FakeRegistryService : IRegistryService
{
    public int RegisterCallCount { get; private set; }

    public int RemoveCallCount { get; private set; }

    public InstalledStateManifest? LastRegisteredState { get; private set; }

    public UninstallPlan? LastRemovedPlan { get; private set; }

    public void RegisterInstallerEntries(ProductManifest product, PackageManifest package, InstalledStateManifest state)
    {
        RegisterCallCount++;
        LastRegisteredState = state;
    }

    public void RemoveInstallerEntries(ProductManifest product, UninstallPlan plan, bool removeFileAssociations, bool removeAutorun)
    {
        RemoveCallCount++;
        LastRemovedPlan = plan;
    }
}
