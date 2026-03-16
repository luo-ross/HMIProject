namespace RS.SetupApp.Core;

public sealed class ShortcutManifest
{
    public string Name { get; set; } = string.Empty;

    public ShortcutLocation Location { get; set; }

    public bool EnabledByDefault { get; set; } = true;

    public string? Description { get; set; }

    public string? IconPath { get; set; }

    public string? Arguments { get; set; }

    public string? FolderName { get; set; }
}
