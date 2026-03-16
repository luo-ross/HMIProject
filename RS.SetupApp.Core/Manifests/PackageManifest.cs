namespace RS.SetupApp.Core;

public sealed class PackageManifest
{
    public string ProductId { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string PackageType { get; set; } = "full";

    public string MainExecutable { get; set; } = string.Empty;

    public string ArchiveFileName { get; set; } = string.Empty;

    public string ArchiveSha256 { get; set; } = string.Empty;

    public string? ReleaseNotes { get; set; }

    public string? MinSupportedVersion { get; set; }

    public DateTimeOffset GeneratedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public List<PackageFileEntry> FileEntries { get; set; } = new();

    public long TotalSizeBytes => FileEntries.Sum(entry => entry.SizeBytes);
}
