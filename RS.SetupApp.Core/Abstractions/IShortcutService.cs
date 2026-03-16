namespace RS.SetupApp.Core;

public interface IShortcutService
{
    IReadOnlyList<RegisteredShortcutState> CreateShortcuts(ProductManifest product, InstalledStateManifest state, bool enabled);

    void RemoveShortcuts(IEnumerable<RegisteredShortcutState> shortcuts);
}
