namespace RS.SetupApp.Core;

public sealed class DefaultSystemPaths : ISystemPaths
{
    public DefaultSystemPaths(string? appBaseDirectory = null)
    {
        AppBaseDirectory = appBaseDirectory ?? AppContext.BaseDirectory;
    }

    public string AppBaseDirectory { get; }

    public string GetPayloadDirectory()
    {
        string payload = Path.Combine(AppBaseDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName);
        return Directory.Exists(payload) ? payload : AppBaseDirectory;
    }

    public string GetDefaultInstallDirectory(ProductManifest product, InstallScope scope)
    {
        string template = !string.IsNullOrWhiteSpace(product.InstallDefaults.DefaultInstallDirectoryOverride)
            ? product.InstallDefaults.DefaultInstallDirectoryOverride!
            : scope == InstallScope.AllUsers
                ? product.InstallDefaults.MachineInstallDirectoryTemplate ?? @"{ProgramFiles}\{Publisher}\{DisplayName}"
                : product.InstallDefaults.UserInstallDirectoryTemplate ?? @"{LocalAppData}\Programs\{Publisher}\{DisplayName}";

        string resolved = SetupPathUtility.ExpandEnvironmentTokens(template);
        return Path.GetFullPath(SetupPathUtility.ApplyTokens(resolved, product));
    }

    public string GetStateManifestPath(string productId, InstallScope scope)
    {
        string root = scope == InstallScope.AllUsers
            ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(root, "RS.SetupApp", "InstalledProducts", SetupPathUtility.SanitizePathSegment(productId), "installed-state.json");
    }

    public string GetRecoveryRoot(string productId, InstallScope scope)
    {
        string root = scope == InstallScope.AllUsers
            ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(root, "RS.SetupApp", "Recovery", SetupPathUtility.SanitizePathSegment(productId));
    }

    public string GetRecoveryDirectory(string productId, Guid operationId, InstallScope scope)
    {
        return Path.Combine(GetRecoveryRoot(productId, scope), operationId.ToString("N"));
    }

    public string GetLogFilePath(string productId, InstallScope scope)
    {
        string root = scope == InstallScope.AllUsers
            ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        return Path.Combine(root, "RS.SetupApp", "Logs", SetupPathUtility.SanitizePathSegment(productId), $"setup-{timestamp}.jsonl");
    }

    public string GetDataDirectory(ProductManifest product, InstallScope installScope, DataDirectoryManifest directory)
    {
        string root = directory.Scope == DataDirectoryScope.CommonAppData || installScope == InstallScope.AllUsers
            ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(root, SetupPathUtility.ApplyTokens(directory.RelativePath, product));
    }

    public string GetShortcutPath(ProductManifest product, ShortcutManifest shortcut, InstallScope scope)
    {
        string root = shortcut.Location switch
        {
            ShortcutLocation.Desktop => Environment.GetFolderPath(scope == InstallScope.AllUsers
                ? Environment.SpecialFolder.CommonDesktopDirectory
                : Environment.SpecialFolder.DesktopDirectory),
            ShortcutLocation.StartMenuRoot => Environment.GetFolderPath(scope == InstallScope.AllUsers
                ? Environment.SpecialFolder.CommonStartMenu
                : Environment.SpecialFolder.StartMenu),
            ShortcutLocation.StartMenuPrograms => Environment.GetFolderPath(scope == InstallScope.AllUsers
                ? Environment.SpecialFolder.CommonPrograms
                : Environment.SpecialFolder.Programs),
            _ => throw new ArgumentOutOfRangeException(nameof(shortcut.Location))
        };

        string fileName = $"{(string.IsNullOrWhiteSpace(shortcut.Name) ? product.DisplayName : shortcut.Name)}.lnk";
        if (shortcut.Location == ShortcutLocation.StartMenuPrograms)
        {
            string folder = string.IsNullOrWhiteSpace(shortcut.FolderName) ? product.DisplayName : shortcut.FolderName!;
            root = Path.Combine(root, SetupPathUtility.SanitizePathSegment(folder));
        }

        return Path.Combine(root, fileName);
    }

    public string GetMaintenanceDirectory(string installDirectory)
    {
        return Path.Combine(installDirectory, SetupRuntimeDefaults.MaintenanceFolderName);
    }

    public string GetTemporaryWorkingDirectory(string productId)
    {
        return Path.Combine(Path.GetTempPath(), "RS.SetupApp", SetupPathUtility.SanitizePathSegment(productId), Guid.NewGuid().ToString("N"));
    }
}
