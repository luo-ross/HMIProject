namespace RS.SetupApp.Core;

public sealed class InstallationOwnershipMarker
{
    public int SchemaVersion { get; set; } = 1;

    public string ProductId { get; set; } = string.Empty;

    public Guid InstallationId { get; set; }

    public InstallScope InstallScope { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
