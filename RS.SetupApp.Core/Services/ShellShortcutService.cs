using System.Runtime.Versioning;
using System.Text.Json;

namespace RS.SetupApp.Core;

[SupportedOSPlatform("windows")]
public sealed class ShellShortcutService : IShortcutService
{
    private readonly ISystemPaths _paths;

    public ShellShortcutService(ISystemPaths paths)
    {
        _paths = paths;
    }

    public string CaptureSnapshot(IEnumerable<RegisteredShortcutState> shortcuts)
    {
        List<ShortcutSnapshot> snapshot = shortcuts
            .Select(shortcut => new ShortcutSnapshot
            {
                Path = shortcut.Path,
                Existed = File.Exists(shortcut.Path),
                Contents = File.Exists(shortcut.Path)
                    ? Convert.ToBase64String(File.ReadAllBytes(shortcut.Path))
                    : null
            })
            .ToList();
        return JsonSerializer.Serialize(snapshot);
    }

    public void RestoreSnapshot(string snapshot)
    {
        List<ShortcutSnapshot> entries = JsonSerializer.Deserialize<List<ShortcutSnapshot>>(snapshot)
            ?? throw new InvalidOperationException("The shortcut snapshot is invalid.");
        foreach (ShortcutSnapshot entry in entries)
        {
            if (File.Exists(entry.Path))
            {
                File.Delete(entry.Path);
            }

            if (!entry.Existed)
            {
                continue;
            }

            string? directory = Path.GetDirectoryName(entry.Path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(entry.Path, Convert.FromBase64String(entry.Contents
                ?? throw new InvalidOperationException("The shortcut snapshot contents are missing.")));
        }
    }

    public void DeleteShortcut(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
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
            DeleteShortcut(shortcut.Path);
        }
    }

    private sealed class ShortcutSnapshot
    {
        public string Path { get; init; } = string.Empty;

        public bool Existed { get; init; }

        public string? Contents { get; init; }
    }
}
