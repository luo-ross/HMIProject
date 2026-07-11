using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Fakes;

public sealed class FakeShortcutService : IShortcutService
{
    public ISystemPaths? Paths { get; set; }

    public int CreateCallCount { get; private set; }

    public int RemoveCallCount { get; private set; }

    public IReadOnlyList<RegisteredShortcutState> LastRemovedShortcuts { get; private set; } =
        Array.Empty<RegisteredShortcutState>();

    public IReadOnlyList<RegisteredShortcutState> CreateShortcuts(ProductManifest product, InstalledStateManifest state, bool enabled)
    {
        CreateCallCount++;
        if (!enabled)
        {
            return Array.Empty<RegisteredShortcutState>();
        }

        return product.Shortcuts
            .Where(item => item.EnabledByDefault)
            .Select(item => new RegisteredShortcutState
            {
                Name = string.IsNullOrWhiteSpace(item.Name) ? product.DisplayName : item.Name,
                Path = Paths?.GetShortcutPath(product, item, state.InstallScope)
                    ?? $"shortcuts\\{item.Location}\\{product.ProductId}.lnk",
                Location = item.Location
            })
            .ToList();
    }

    public void RemoveShortcuts(IEnumerable<RegisteredShortcutState> shortcuts)
    {
        RemoveCallCount++;
        LastRemovedShortcuts = shortcuts.ToArray();
    }
}
