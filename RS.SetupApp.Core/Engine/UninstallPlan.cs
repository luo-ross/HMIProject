namespace RS.SetupApp.Core;

public sealed record UninstallTarget(string Path, SetupPathPurpose Purpose);

public sealed record UninstallPlan(
    string InstallDirectory,
    string StateManifestPath,
    IReadOnlyList<UninstallTarget> FileSystemTargets,
    IReadOnlyList<RegisteredShortcutState> Shortcuts)
{
    public string ProductId { get; init; } = string.Empty;

    public Guid InstallationId { get; init; }

    public InstallScope InstallScope { get; init; }

    public string MainExecutablePath { get; init; } = string.Empty;

    public string MaintenanceDirectory { get; init; } = string.Empty;

    public string RecoveryRoot { get; init; } = string.Empty;

    public string? AutorunEntryName { get; init; }

    public IReadOnlyList<RegisteredFileAssociationState> FileAssociations { get; init; } =
        Array.Empty<RegisteredFileAssociationState>();
}
