namespace RS.SetupApp.Core;

public sealed class InstalledStateManifest
{
    public string ProductId { get; set; } = string.Empty;

    public Guid InstallationId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Publisher { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public InstallScope InstallScope { get; set; }

    public string InstallDirectory { get; set; } = string.Empty;

    public string MainExecutablePath { get; set; } = string.Empty;

    public string StateManifestPath { get; set; } = string.Empty;

    public string MaintenanceDirectory { get; set; } = string.Empty;

    public string MaintenanceExecutablePath { get; set; } = string.Empty;

    public string MaintenanceProductManifestPath { get; set; } = string.Empty;

    public string? MaintenancePackageManifestPath { get; set; }

    public string? MaintenancePackagePath { get; set; }

    public string? PendingBackupDirectory { get; set; }

    public string? LastBackupDirectory { get; set; }

    public string? UninstallRegistryPath { get; set; }

    public string? AutorunEntryName { get; set; }

    public bool AutorunEnabled { get; set; }

    public DateTimeOffset InstalledAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastSuccessfulInstallAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public List<RegisteredShortcutState> Shortcuts { get; set; } = new();

    public List<RegisteredFileAssociationState> FileAssociations { get; set; } = new();

    public Dictionary<string, string> DataDirectories { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
