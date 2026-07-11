namespace RS.SetupApp.Core;

public interface IShortcutService
{
    string CaptureSnapshot(IEnumerable<RegisteredShortcutState> shortcuts);

    void RestoreSnapshot(string snapshot);

    void DeleteShortcut(string path);

    IReadOnlyList<RegisteredShortcutState> CreateShortcuts(ProductManifest product, InstalledStateManifest state, bool enabled);

    void RemoveShortcuts(IEnumerable<RegisteredShortcutState> shortcuts);
}
