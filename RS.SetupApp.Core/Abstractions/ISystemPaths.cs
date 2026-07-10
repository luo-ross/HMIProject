namespace RS.SetupApp.Core;

public interface ISystemPaths
{
    string AppBaseDirectory { get; }

    string GetPayloadDirectory();

    string GetDefaultInstallDirectory(ProductManifest product, InstallScope scope);

    string GetStateManifestPath(string productId, InstallScope scope);

    string GetRecoveryRoot(string productId, InstallScope scope);

    string GetRecoveryDirectory(string productId, Guid operationId, InstallScope scope);

    string GetLogFilePath(string productId, InstallScope scope);

    string GetDataDirectory(ProductManifest product, InstallScope installScope, DataDirectoryManifest directory);

    string GetShortcutPath(ProductManifest product, ShortcutManifest shortcut, InstallScope scope);

    string GetMaintenanceDirectory(string installDirectory);

    string GetTemporaryWorkingDirectory(string productId);
}
