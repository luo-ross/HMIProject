namespace RS.SetupApp.Core;

public sealed class SetupTransactionJournal
{
    public required Guid OperationId { get; init; }

    public required string ProductId { get; init; }

    public required InstallScope Scope { get; init; }

    public required SetupMode Mode { get; init; }

    public required string InstallDirectory { get; init; }

    public required string RecoveryDirectory { get; init; }

    public SetupTransactionPhase Phase { get; set; }

    public List<string> CompletedSteps { get; init; } = [];

    public required DateTimeOffset StartedAtUtc { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; set; }

    public string? PrimaryError { get; set; }

    public List<string> RecoveryErrors { get; init; } = [];

    public List<SetupCompensationRecord> Compensations { get; init; } = [];
}
