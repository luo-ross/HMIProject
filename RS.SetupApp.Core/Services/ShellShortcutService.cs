using System.Runtime.Versioning;

namespace RS.SetupApp.Core;

[SupportedOSPlatform("windows")]
public sealed class ShellShortcutService : IShortcutService
{
    private readonly ISystemPaths _paths;

    public ShellShortcutService(ISystemPaths paths)
    {
        _paths = paths;
    }

    public IReadOnlyList<RegisteredShortcutState> CreateShortcuts(ProductManifest product, InstalledStateManifest state, bool enabled)
    {
        List<RegisteredShortcutState> created = new();
        if (!enabled)
        {
            return created;
        }

        Type shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new PlatformNotSupportedException("WScript.Shell COM is not available.");

        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Unable to create WScript.Shell instance.");

        foreach (ShortcutManifest shortcut in product.Shortcuts.Where(item => item.EnabledByDefault))
        {
            string shortcutPath = _paths.GetShortcutPath(product, shortcut, state.InstallScope);
            string? directory = Path.GetDirectoryName(shortcutPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            dynamic link = shell.CreateShortcut(shortcutPath);
            link.TargetPath = state.MainExecutablePath;
            link.WorkingDirectory = state.InstallDirectory;
            link.Arguments = shortcut.Arguments ?? string.Empty;
            link.Description = string.IsNullOrWhiteSpace(shortcut.Description)
                ? $"{product.DisplayName} shortcut"
                : shortcut.Description;
            link.IconLocation = string.IsNullOrWhiteSpace(shortcut.IconPath)
                ? state.MainExecutablePath
                : shortcut.IconPath!.Replace("{installDir}", state.InstallDirectory, StringComparison.OrdinalIgnoreCase)
                    .Replace("{exe}", state.MainExecutablePath, StringComparison.OrdinalIgnoreCase);
            link.Save();

            created.Add(new RegisteredShortcutState
            {
                Name = string.IsNullOrWhiteSpace(shortcut.Name) ? product.DisplayName : shortcut.Name,
                Path = shortcutPath,
                Location = shortcut.Location
            });
        }

        return created;
    }

    public void RemoveShortcuts(IEnumerable<RegisteredShortcutState> shortcuts)
    {
        foreach (RegisteredShortcutState shortcut in shortcuts)
        {
            if (File.Exists(shortcut.Path))
            {
                File.Delete(shortcut.Path);
            }
        }
    }
}
