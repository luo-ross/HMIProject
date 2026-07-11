namespace RS.SetupApp.Core;

public sealed class SetupCompensationRecord
{
    public required Guid Id { get; init; }

    public required SetupCompensationKind Kind { get; init; }

    public required string Target { get; init; }

    public string? Backup { get; init; }

    public Dictionary<string, string> Metadata { get; init; } = [];

    public bool Applied { get; set; }

    public bool Reverted { get; set; }
}
