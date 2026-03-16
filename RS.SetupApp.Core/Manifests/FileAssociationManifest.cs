namespace RS.SetupApp.Core;

public sealed class FileAssociationManifest
{
    public string Extension { get; set; } = string.Empty;

    public string ProgId { get; set; } = string.Empty;

    public string FriendlyName { get; set; } = string.Empty;

    public string? IconPath { get; set; }

    public string? CommandTemplate { get; set; }
}
