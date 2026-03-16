namespace RS.SetupApp.Core;

public sealed class SetupExecutionContext
{
    public ProductManifest? Product { get; set; }

    public PackageManifest? Package { get; set; }

    public UpdateFeedManifest? UpdateFeed { get; set; }

    public required RuntimeOptions Options { get; init; }

    public required SetupServices Services { get; init; }

    public required string ProductManifestPath { get; init; }

    public required string PayloadDirectory { get; init; }

    public string? SchemaPath { get; set; }

    public InstalledStateManifest? ExistingState { get; set; }

    public InstalledStateManifest? ResultState { get; set; }

    public ISetupLogger? Logger { get; set; }

    public SetupMode ActualMode { get; set; } = SetupMode.Install;

    public InstallScope EffectiveScope { get; set; } = InstallScope.CurrentUser;

    public string? InstallDirectory { get; set; }

    public bool RequiresOnlinePackage { get; set; }

    public List<ISetupExtension> Extensions { get; } = new();

    public string? PackagePath { get; set; }

    public string? PackageManifestPath { get; set; }

    public string? UpdateManifestPath { get; set; }

    public string? WorkingDirectory { get; set; }

    public string? ExtractionDirectory { get; set; }

    public string? BackupDirectory { get; set; }
}
