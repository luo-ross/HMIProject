namespace RS.SetupApp.Core;

public sealed class ProductManifest
{
    public string ProductId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public LocalizedTextManifest? DisplayNameLocalized { get; set; }

    public string Publisher { get; set; } = string.Empty;

    public string MainExecutable { get; set; } = string.Empty;

    public string? MainProcessName { get; set; }

    public string? UpgradeCode { get; set; }

    public string? SupportUrl { get; set; }

    public string? UpdateInfoUrl { get; set; }

    public BrandingManifest Branding { get; set; } = new();

    public InstallDefaultsManifest InstallDefaults { get; set; } = new();

    public UpdateSettingsManifest Update { get; set; } = new();

    public UninstallPolicyManifest Uninstall { get; set; } = new();

    public List<FileAssociationManifest> FileAssociations { get; set; } = new();

    public List<ShortcutManifest> Shortcuts { get; set; } =
    [
        new ShortcutManifest { Location = ShortcutLocation.Desktop, EnabledByDefault = true },
        new ShortcutManifest { Location = ShortcutLocation.StartMenuPrograms, EnabledByDefault = true }
    ];

    public List<DataDirectoryManifest> DataDirectories { get; set; } = new();

    public List<ExtensionRegistrationManifest> Extensions { get; set; } = new();

    public string GetMainProcessName()
    {
        if (!string.IsNullOrWhiteSpace(MainProcessName))
        {
            return MainProcessName!;
        }

        return Path.GetFileNameWithoutExtension(MainExecutable);
    }
}
