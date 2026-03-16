namespace RS.SetupApp.Core;

public sealed class PackageFileEntry
{
    public string RelativePath { get; set; } = string.Empty;

    public string Sha256 { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
}
