namespace RS.SetupApp.Core;

public sealed class UpdateSettingsManifest
{
    public bool AllowOnlineUpdate { get; set; }

    public string Channel { get; set; } = "stable";

    public string? ManifestUrl { get; set; }
}
