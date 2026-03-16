namespace RS.SetupApp.Core;

public sealed class InstallDefaultsManifest
{
    public InstallScope DefaultScope { get; set; } = InstallScope.CurrentUser;

    public string? DefaultInstallDirectoryOverride { get; set; }

    public string? UserInstallDirectoryTemplate { get; set; }

    public string? MachineInstallDirectoryTemplate { get; set; }

    public bool CreateShortcutsByDefault { get; set; } = true;

    public bool EnableAutoStartByDefault { get; set; }

    public bool AllowSilentInstall { get; set; } = true;

    public bool AllowRepair { get; set; } = true;

    public bool AllowOverwrite { get; set; } = true;

    public bool AllowMachineInstall { get; set; } = true;

    public long MinimumFreeSpaceBytes { get; set; } = 200 * 1024 * 1024;
}
