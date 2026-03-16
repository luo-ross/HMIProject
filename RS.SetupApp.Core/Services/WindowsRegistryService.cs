using System.Runtime.Versioning;
using Microsoft.Win32;

namespace RS.SetupApp.Core;

[SupportedOSPlatform("windows")]
public sealed class WindowsRegistryService : IRegistryService
{
    private const string MarkerValueName = "RS.SetupAppProductId";

    public void RegisterInstallerEntries(ProductManifest product, PackageManifest package, InstalledStateManifest state)
    {
        RegisterUninstallEntry(product, package, state);
        RegisterAutorun(product, state);
        RegisterFileAssociations(product, state);
    }

    public void RemoveInstallerEntries(ProductManifest product, InstalledStateManifest state, bool removeFileAssociations, bool removeAutorun)
    {
        if (removeAutorun)
        {
            RemoveAutorun(state);
        }

        RemoveUninstallEntry(product, state);

        if (removeFileAssociations)
        {
            RemoveFileAssociations(product, state);
        }
    }

    private static RegistryKey GetHive(InstallScope scope)
    {
        return scope == InstallScope.AllUsers ? Registry.LocalMachine : Registry.CurrentUser;
    }

    private static string GetHiveName(InstallScope scope)
    {
        return scope == InstallScope.AllUsers ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";
    }

    private void RegisterUninstallEntry(ProductManifest product, PackageManifest package, InstalledStateManifest state)
    {
        using RegistryKey uninstallKey = GetHive(state.InstallScope).CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{product.ProductId}", writable: true)
            ?? throw new InvalidOperationException("Unable to create uninstall registry entry.");

        string scopeValue = state.InstallScope == InstallScope.AllUsers ? "machine" : "user";
        string uninstallCommand = $"{SetupPathUtility.Quote(state.MaintenanceExecutablePath)} --mode uninstall --worker --scope {scopeValue} --product {SetupPathUtility.Quote(state.MaintenanceProductManifestPath)} --skip-launch";
        string repairCommand = $"{SetupPathUtility.Quote(state.MaintenanceExecutablePath)} --mode repair --worker --scope {scopeValue} --product {SetupPathUtility.Quote(state.MaintenanceProductManifestPath)} --skip-launch";

        uninstallKey.SetValue("DisplayName", product.DisplayName);
        uninstallKey.SetValue("DisplayVersion", package.Version);
        uninstallKey.SetValue("Publisher", product.Publisher);
        uninstallKey.SetValue("InstallLocation", state.InstallDirectory);
        uninstallKey.SetValue("DisplayIcon", state.MainExecutablePath);
        uninstallKey.SetValue("UninstallString", uninstallCommand);
        uninstallKey.SetValue("QuietUninstallString", $"{uninstallCommand} --silent");
        uninstallKey.SetValue("ModifyPath", repairCommand);
        uninstallKey.SetValue("EstimatedSize", (int)Math.Min(int.MaxValue, package.TotalSizeBytes / 1024));
        uninstallKey.SetValue("NoModify", product.InstallDefaults.AllowRepair ? 0 : 1);
        uninstallKey.SetValue("NoRepair", product.InstallDefaults.AllowRepair ? 0 : 1);
        uninstallKey.SetValue("URLInfoAbout", product.SupportUrl ?? string.Empty);
        uninstallKey.SetValue("URLUpdateInfo", product.UpdateInfoUrl ?? string.Empty);
        uninstallKey.SetValue(MarkerValueName, product.ProductId);

        state.UninstallRegistryPath = $@"{GetHiveName(state.InstallScope)}\Software\Microsoft\Windows\CurrentVersion\Uninstall\{product.ProductId}";
    }

    private void RemoveUninstallEntry(ProductManifest product, InstalledStateManifest state)
    {
        RegistryKey hive = GetHive(state.InstallScope);
        string keyPath = $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{product.ProductId}";
        if (hive.OpenSubKey(keyPath) != null)
        {
            hive.DeleteSubKeyTree(keyPath, throwOnMissingSubKey: false);
        }
    }

    private void RegisterAutorun(ProductManifest product, InstalledStateManifest state)
    {
        string runPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        using RegistryKey runKey = GetHive(state.InstallScope).CreateSubKey(runPath, writable: true)
            ?? throw new InvalidOperationException("Unable to create autorun key.");

        string entryName = string.IsNullOrWhiteSpace(state.AutorunEntryName)
            ? SetupPathUtility.SanitizePathSegment(product.ProductId)
            : state.AutorunEntryName!;

        if (state.AutorunEnabled)
        {
            runKey.SetValue(entryName, SetupPathUtility.Quote(state.MainExecutablePath));
            state.AutorunEntryName = entryName;
        }
        else if (!string.IsNullOrWhiteSpace(entryName))
        {
            runKey.DeleteValue(entryName, throwOnMissingValue: false);
        }
    }

    private void RemoveAutorun(InstalledStateManifest state)
    {
        if (string.IsNullOrWhiteSpace(state.AutorunEntryName))
        {
            return;
        }

        string runPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        using RegistryKey? runKey = GetHive(state.InstallScope).OpenSubKey(runPath, writable: true);
        runKey?.DeleteValue(state.AutorunEntryName, throwOnMissingValue: false);
    }

    private void RegisterFileAssociations(ProductManifest product, InstalledStateManifest state)
    {
        RegistryKey hive = GetHive(state.InstallScope);
        state.FileAssociations.Clear();

        foreach (FileAssociationManifest association in product.FileAssociations)
        {
            using RegistryKey extensionKey = hive.CreateSubKey($@"Software\Classes\{association.Extension}", writable: true)
                ?? throw new InvalidOperationException($"Unable to create registry key for '{association.Extension}'.");
            extensionKey.SetValue(string.Empty, association.ProgId);
            extensionKey.SetValue(MarkerValueName, product.ProductId);

            using RegistryKey openWithKey = extensionKey.CreateSubKey("OpenWithProgids", writable: true)
                ?? throw new InvalidOperationException("Unable to create OpenWithProgids registry key.");
            openWithKey.SetValue(association.ProgId, string.Empty);

            using RegistryKey progIdKey = hive.CreateSubKey($@"Software\Classes\{association.ProgId}", writable: true)
                ?? throw new InvalidOperationException($"Unable to create registry key for '{association.ProgId}'.");
            progIdKey.SetValue(string.Empty, association.FriendlyName);
            progIdKey.SetValue(MarkerValueName, product.ProductId);

            string iconPath = string.IsNullOrWhiteSpace(association.IconPath)
                ? state.MainExecutablePath
                : association.IconPath!.Replace("{installDir}", state.InstallDirectory, StringComparison.OrdinalIgnoreCase)
                    .Replace("{exe}", state.MainExecutablePath, StringComparison.OrdinalIgnoreCase);

            using RegistryKey iconKey = progIdKey.CreateSubKey("DefaultIcon", writable: true)
                ?? throw new InvalidOperationException("Unable to create DefaultIcon registry key.");
            iconKey.SetValue(string.Empty, iconPath);

            using RegistryKey commandKey = progIdKey.CreateSubKey(@"shell\open\command", writable: true)
                ?? throw new InvalidOperationException("Unable to create command registry key.");
            string openCommand = SetupPathUtility.ExpandCommandTemplate(association.CommandTemplate, state.MainExecutablePath);
            commandKey.SetValue(string.Empty, openCommand);

            state.FileAssociations.Add(new RegisteredFileAssociationState
            {
                Extension = association.Extension,
                ProgId = association.ProgId,
                Command = openCommand,
                CommandRegistryPath = $@"Software\Classes\{association.ProgId}\shell\open\command"
            });
        }
    }

    private void RemoveFileAssociations(ProductManifest product, InstalledStateManifest state)
    {
        RegistryKey hive = GetHive(state.InstallScope);
        foreach (RegisteredFileAssociationState association in state.FileAssociations)
        {
            string extensionPath = $@"Software\Classes\{association.Extension}";
            using RegistryKey? extensionKey = hive.OpenSubKey(extensionPath, writable: true);
            if (extensionKey?.GetValue(MarkerValueName)?.ToString() == product.ProductId)
            {
                hive.DeleteSubKeyTree(extensionPath, throwOnMissingSubKey: false);
            }

            string progIdPath = $@"Software\Classes\{association.ProgId}";
            using RegistryKey? progIdKey = hive.OpenSubKey(progIdPath, writable: true);
            using RegistryKey? commandKey = hive.OpenSubKey(association.CommandRegistryPath);
            string? currentCommand = commandKey?.GetValue(string.Empty)?.ToString();
            string? currentExecutable = SetupPathUtility.TryExtractExecutablePath(currentCommand);
            bool pointsToProduct = !string.IsNullOrWhiteSpace(currentExecutable) &&
                string.Equals(Path.GetFullPath(currentExecutable), Path.GetFullPath(state.MainExecutablePath), StringComparison.OrdinalIgnoreCase);
            if (progIdKey?.GetValue(MarkerValueName)?.ToString() == product.ProductId && pointsToProduct)
            {
                hive.DeleteSubKeyTree(progIdPath, throwOnMissingSubKey: false);
            }
        }
    }
}
