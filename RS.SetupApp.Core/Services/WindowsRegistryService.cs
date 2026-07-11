using System.Runtime.Versioning;
using System.Text.Json;
using Microsoft.Win32;

namespace RS.SetupApp.Core;

[SupportedOSPlatform("windows")]
public sealed class WindowsRegistryService : IRegistryService
{
    private const string MarkerValueName = "RS.SetupAppProductId";

    public string CaptureInstallerEntriesSnapshot(ProductManifest product, InstalledStateManifest state)
    {
        return CaptureInstallerEntriesSnapshot(
            product,
            state.InstallScope,
            state.AutorunEntryName,
            product.FileAssociations.Select(item => new RegisteredFileAssociationState
            {
                Extension = item.Extension,
                ProgId = item.ProgId
            }));
    }

    public string CaptureInstallerEntriesSnapshot(ProductManifest product, UninstallPlan plan)
    {
        return CaptureInstallerEntriesSnapshot(
            product,
            plan.InstallScope,
            plan.AutorunEntryName,
            plan.FileAssociations);
    }

    public void RestoreInstallerEntriesSnapshot(string snapshot)
    {
        RegistryInstallerSnapshot document = JsonSerializer.Deserialize<RegistryInstallerSnapshot>(snapshot)
            ?? throw new InvalidOperationException("The registry snapshot is invalid.");
        RegistryKey hive = GetHive(document.Scope);
        foreach (RegistryKeySnapshot key in document.Keys)
        {
            RestoreKey(hive, key);
        }

        if (document.Autorun != null)
        {
            RestoreValue(hive, document.Autorun);
        }
    }

    public void DeleteValue(InstallScope scope, string keyPath, string valueName)
    {
        using RegistryKey? key = GetHive(scope).OpenSubKey(keyPath, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }

    public void RegisterInstallerEntries(ProductManifest product, PackageManifest package, InstalledStateManifest state)
    {
        RegisterUninstallEntry(product, package, state);
        RegisterAutorun(product, state);
        RegisterFileAssociations(product, state);
    }

    public void RemoveInstallerEntries(ProductManifest product, UninstallPlan plan, bool removeFileAssociations, bool removeAutorun)
    {
        if (removeAutorun)
        {
            RemoveAutorun(plan);
        }

        RemoveUninstallEntry(product, plan);

        if (removeFileAssociations)
        {
            RemoveFileAssociations(product, plan);
        }
    }

    private string CaptureInstallerEntriesSnapshot(
        ProductManifest product,
        InstallScope scope,
        string? autorunEntryName,
        IEnumerable<RegisteredFileAssociationState> associations)
    {
        RegistryKey hive = GetHive(scope);
        RegistryInstallerSnapshot snapshot = new() { Scope = scope };
        snapshot.Keys.Add(CaptureKey(
            hive,
            $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{product.ProductId}"));
        foreach (RegisteredFileAssociationState association in associations)
        {
            snapshot.Keys.Add(CaptureKey(hive, $@"Software\Classes\{association.Extension}"));
            snapshot.Keys.Add(CaptureKey(hive, $@"Software\Classes\{association.ProgId}"));
        }

        if (!string.IsNullOrWhiteSpace(autorunEntryName))
        {
            snapshot.Autorun = CaptureValue(
                hive,
                @"Software\Microsoft\Windows\CurrentVersion\Run",
                autorunEntryName);
        }

        return JsonSerializer.Serialize(snapshot);
    }

    private static RegistryKeySnapshot CaptureKey(RegistryKey hive, string path)
    {
        using RegistryKey? key = hive.OpenSubKey(path, writable: false);
        return key == null
            ? new RegistryKeySnapshot { Path = path, Existed = false }
            : CaptureKey(path, key);
    }

    private static RegistryKeySnapshot CaptureKey(string path, RegistryKey key)
    {
        RegistryKeySnapshot snapshot = new() { Path = path, Existed = true };
        foreach (string name in key.GetValueNames())
        {
            snapshot.Values.Add(CaptureValue(key, name));
        }

        foreach (string name in key.GetSubKeyNames())
        {
            using RegistryKey? child = key.OpenSubKey(name, writable: false);
            if (child != null)
            {
                snapshot.Children.Add(CaptureKey(name, child));
            }
        }

        return snapshot;
    }

    private static RegistryValueSnapshot CaptureValue(RegistryKey hive, string path, string valueName)
    {
        using RegistryKey? key = hive.OpenSubKey(path, writable: false);
        if (key == null || !key.GetValueNames().Contains(valueName, StringComparer.OrdinalIgnoreCase))
        {
            return new RegistryValueSnapshot { Path = path, Name = valueName, Existed = false };
        }

        RegistryValueSnapshot snapshot = CaptureValue(key, valueName);
        snapshot.Path = path;
        return snapshot;
    }

    private static RegistryValueSnapshot CaptureValue(RegistryKey key, string valueName)
    {
        object? value = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        RegistryValueKind kind = key.GetValueKind(valueName);
        RegistryValueSnapshot snapshot = new()
        {
            Name = valueName,
            Existed = true,
            Kind = kind
        };
        switch (value)
        {
            case byte[] bytes:
                snapshot.BinaryValue = Convert.ToBase64String(bytes);
                break;
            case string[] strings:
                snapshot.MultiStringValue = strings;
                break;
            case int integer:
                snapshot.IntValue = integer;
                break;
            case long longValue:
                snapshot.LongValue = longValue;
                break;
            default:
                snapshot.StringValue = value?.ToString();
                break;
        }

        return snapshot;
    }

    private static void RestoreKey(RegistryKey hive, RegistryKeySnapshot snapshot)
    {
        if (hive.OpenSubKey(snapshot.Path, writable: false) != null)
        {
            hive.DeleteSubKeyTree(snapshot.Path, throwOnMissingSubKey: false);
        }

        if (!snapshot.Existed)
        {
            return;
        }

        using RegistryKey key = hive.CreateSubKey(snapshot.Path, writable: true)
            ?? throw new InvalidOperationException($"Unable to restore registry key '{snapshot.Path}'.");
        RestoreKeyContents(key, snapshot);
    }

    private static void RestoreKeyContents(RegistryKey key, RegistryKeySnapshot snapshot)
    {
        foreach (RegistryValueSnapshot value in snapshot.Values)
        {
            key.SetValue(value.Name, GetValue(value), value.Kind);
        }

        foreach (RegistryKeySnapshot child in snapshot.Children)
        {
            using RegistryKey childKey = key.CreateSubKey(child.Path, writable: true)
                ?? throw new InvalidOperationException($"Unable to restore registry key '{child.Path}'.");
            RestoreKeyContents(childKey, child);
        }
    }

    private static void RestoreValue(RegistryKey hive, RegistryValueSnapshot snapshot)
    {
        using RegistryKey? existing = hive.OpenSubKey(snapshot.Path, writable: true);
        if (existing != null)
        {
            existing.DeleteValue(snapshot.Name, throwOnMissingValue: false);
        }

        if (!snapshot.Existed)
        {
            return;
        }

        using RegistryKey key = hive.CreateSubKey(snapshot.Path, writable: true)
            ?? throw new InvalidOperationException($"Unable to restore registry key '{snapshot.Path}'.");
        key.SetValue(snapshot.Name, GetValue(snapshot), snapshot.Kind);
    }

    private static object GetValue(RegistryValueSnapshot snapshot)
    {
        return snapshot.Kind switch
        {
            RegistryValueKind.Binary or RegistryValueKind.None => Convert.FromBase64String(snapshot.BinaryValue ?? string.Empty),
            RegistryValueKind.MultiString => snapshot.MultiStringValue ?? Array.Empty<string>(),
            RegistryValueKind.DWord => snapshot.IntValue,
            RegistryValueKind.QWord => snapshot.LongValue,
            _ => snapshot.StringValue ?? string.Empty
        };
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
        string installDirectoryArgument = $"--install-dir {SetupPathUtility.Quote(state.InstallDirectory)}";
        string uninstallCommand = $"{SetupPathUtility.Quote(state.MaintenanceExecutablePath)} --mode uninstall --worker --scope {scopeValue} --product {SetupPathUtility.Quote(state.MaintenanceProductManifestPath)} {installDirectoryArgument} --skip-launch";
        string repairCommand = $"{SetupPathUtility.Quote(state.MaintenanceExecutablePath)} --mode repair --worker --scope {scopeValue} --product {SetupPathUtility.Quote(state.MaintenanceProductManifestPath)} {installDirectoryArgument} --skip-launch";

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

    private void RemoveUninstallEntry(ProductManifest product, UninstallPlan plan)
    {
        RegistryKey hive = GetHive(plan.InstallScope);
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

    private void RemoveAutorun(UninstallPlan plan)
    {
        if (string.IsNullOrWhiteSpace(plan.AutorunEntryName))
        {
            return;
        }

        string runPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        using RegistryKey? runKey = GetHive(plan.InstallScope).OpenSubKey(runPath, writable: true);
        runKey?.DeleteValue(plan.AutorunEntryName, throwOnMissingValue: false);
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

    private void RemoveFileAssociations(ProductManifest product, UninstallPlan plan)
    {
        RegistryKey hive = GetHive(plan.InstallScope);
        foreach (RegisteredFileAssociationState association in plan.FileAssociations)
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
                string.Equals(Path.GetFullPath(currentExecutable), Path.GetFullPath(plan.MainExecutablePath), StringComparison.OrdinalIgnoreCase);
            if (progIdKey?.GetValue(MarkerValueName)?.ToString() == product.ProductId && pointsToProduct)
            {
                hive.DeleteSubKeyTree(progIdPath, throwOnMissingSubKey: false);
            }
        }
    }

    private sealed class RegistryInstallerSnapshot
    {
        public InstallScope Scope { get; init; }

        public List<RegistryKeySnapshot> Keys { get; init; } = [];

        public RegistryValueSnapshot? Autorun { get; set; }
    }

    private sealed class RegistryKeySnapshot
    {
        public string Path { get; init; } = string.Empty;

        public bool Existed { get; init; }

        public List<RegistryValueSnapshot> Values { get; init; } = [];

        public List<RegistryKeySnapshot> Children { get; init; } = [];
    }

    private sealed class RegistryValueSnapshot
    {
        public string Path { get; set; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public bool Existed { get; init; }

        public RegistryValueKind Kind { get; init; }

        public string? StringValue { get; set; }

        public string? BinaryValue { get; set; }

        public string[]? MultiStringValue { get; set; }

        public int IntValue { get; set; }

        public long LongValue { get; set; }
    }
}
