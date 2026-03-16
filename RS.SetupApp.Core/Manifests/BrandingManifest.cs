namespace RS.SetupApp.Core;

public sealed class BrandingManifest
{
    public string AccentColor { get; set; } = "#0F766E";

    public string? IconPath { get; set; }

    public string? LicensePath { get; set; }

    public string? WelcomeText { get; set; }

    public LocalizedTextManifest? WelcomeTextLocalized { get; set; }

    public string SupportLinkText { get; set; } = "Support";

    public LocalizedTextManifest? SupportLinkTextLocalized { get; set; }

    public string UpdateLinkText { get; set; } = "Release notes";

    public LocalizedTextManifest? UpdateLinkTextLocalized { get; set; }

    public string? TermsUrl { get; set; }

    public string? PrivacyUrl { get; set; }
}
