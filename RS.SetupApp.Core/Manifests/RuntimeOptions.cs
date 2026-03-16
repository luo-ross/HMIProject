namespace RS.SetupApp.Core;

public sealed class RuntimeOptions
{
    public SetupMode Mode { get; set; } = SetupMode.Install;

    public InstallScope? Scope { get; set; }

    public bool Silent { get; set; }

    public bool PurgeData { get; set; }

    public bool NoShortcuts { get; set; }

    public bool NoAutostart { get; set; }

    public bool Worker { get; set; }

    public bool Elevated { get; set; }

    public bool LaunchAfterInstall { get; set; }

    public bool SkipLaunch { get; set; }

    public string? ProductManifestPath { get; set; }

    public string? PackagePath { get; set; }

    public string? PackageManifestPath { get; set; }

    public string? UpdateManifestPath { get; set; }

    public string? InstallDirectory { get; set; }

    public string? LogPath { get; set; }

    public string? Channel { get; set; }
}
