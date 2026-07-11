namespace RS.SetupApp.Core;

public sealed class UpdateSettingsManifest
{
    public bool AllowOnlineUpdate { get; set; }

    public bool RequireHttps { get; set; } = true;

    public bool RequireSignature { get; set; }

    public string? TrustedPublicKeyPath { get; set; }

    public string Channel { get; set; } = "stable";

    public string? ManifestUrl { get; set; }
}
