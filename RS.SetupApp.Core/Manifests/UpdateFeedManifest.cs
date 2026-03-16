namespace RS.SetupApp.Core;

public sealed class UpdateFeedManifest
{
    public string ProductId { get; set; } = string.Empty;

    public string Channel { get; set; } = "stable";

    public string Version { get; set; } = string.Empty;

    public string PackageUrl { get; set; } = string.Empty;

    public string PackageManifestUrl { get; set; } = string.Empty;

    public string PackageSha256 { get; set; } = string.Empty;

    public string? ReleaseNotes { get; set; }
}
