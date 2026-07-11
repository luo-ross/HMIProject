using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Fakes;

public sealed class FakeRegistryService : IRegistryService
{
    public string CurrentSnapshot { get; set; } = "empty";

    public Exception? RegisterException { get; set; }

    public int RestoreSnapshotCallCount { get; private set; }
    public int RegisterCallCount { get; private set; }

    public int RemoveCallCount { get; private set; }

    public InstalledStateManifest? LastRegisteredState { get; private set; }

    public UninstallPlan? LastRemovedPlan { get; private set; }

    public string CaptureInstallerEntriesSnapshot(ProductManifest product, InstalledStateManifest state)
    {
        return CurrentSnapshot;
    }

    public string CaptureInstallerEntriesSnapshot(ProductManifest product, UninstallPlan plan)
    {
        return CurrentSnapshot;
    }

    public void RestoreInstallerEntriesSnapshot(string snapshot)
    {
        RestoreSnapshotCallCount++;
        CurrentSnapshot = snapshot;
    }

    public void DeleteValue(InstallScope scope, string keyPath, string valueName)
    {
    }

    public void RegisterInstallerEntries(ProductManifest product, PackageManifest package, InstalledStateManifest state)
    {
        RegisterCallCount++;
        LastRegisteredState = state;
        CurrentSnapshot = "registered";
        if (RegisterException != null)
        {
            throw RegisterException;
        }
    }

    public void RemoveInstallerEntries(ProductManifest product, UninstallPlan plan, bool removeFileAssociations, bool removeAutorun)
    {
        RemoveCallCount++;
        LastRemovedPlan = plan;
        CurrentSnapshot = "removed";
    }
}
