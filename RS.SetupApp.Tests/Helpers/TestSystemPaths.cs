using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Helpers;

public sealed class TestSystemPaths : ISystemPaths
{
    public TestSystemPaths(string rootPath)
    {
        RootPath = rootPath;
        AppBaseDirectory = Path.Combine(rootPath, "runtime");
        Directory.CreateDirectory(AppBaseDirectory);
    }

    public string RootPath { get; }

    public string AppBaseDirectory { get; }

    public string GetPayloadDirectory()
    {
        string payloadDirectory = Path.Combine(AppBaseDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName);
        Directory.CreateDirectory(payloadDirectory);
        return payloadDirectory;
    }

    public string GetDefaultInstallDirectory(ProductManifest product, InstallScope scope)
    {
        return Path.Combine(RootPath, "install", scope.ToString(), product.ProductId);
    }

    public string GetStateManifestPath(string productId, InstallScope scope)
    {
        return Path.Combine(RootPath, "state", scope.ToString(), productId, "installed-state.json");
    }

    public string GetRecoveryRoot(string productId, InstallScope scope)
    {
        return Path.Combine(RootPath, "recovery", scope.ToString(), productId);
    }

    public string GetRecoveryDirectory(string productId, Guid operationId, InstallScope scope)
    {
        return Path.Combine(GetRecoveryRoot(productId, scope), operationId.ToString("N"));
    }

    public string GetLogFilePath(string productId, InstallScope scope)
    {
        return Path.Combine(RootPath, "logs", scope.ToString(), $"{productId}.jsonl");
    }

    public string GetDataDirectory(ProductManifest product, InstallScope installScope, DataDirectoryManifest directory)
    {
        return Path.Combine(RootPath, "data", directory.Scope.ToString(), product.ProductId, directory.RelativePath);
    }

    public string GetShortcutPath(ProductManifest product, ShortcutManifest shortcut, InstallScope scope)
    {
        return Path.Combine(RootPath, "shortcuts", scope.ToString(), $"{product.ProductId}-{shortcut.Location}.lnk");
    }

    public string GetMaintenanceDirectory(string installDirectory)
    {
        return Path.Combine(installDirectory, SetupRuntimeDefaults.MaintenanceFolderName);
    }

    public string GetTemporaryWorkingDirectory(string productId)
    {
        return Path.Combine(RootPath, "temp", productId, Guid.NewGuid().ToString("N"));
    }
}
