namespace RS.SetupApp.Core;

public sealed class SetupOperationResult
{
    public SetupOperationStatus Status { get; init; }

    public bool Succeeded => Status == SetupOperationStatus.Succeeded;

    public string? FailureCode { get; init; }

    public Exception? PrimaryError { get; init; }

    public IReadOnlyList<string> RecoveryErrors { get; init; } = [];

    public Guid OperationId { get; init; }

    public SetupMode Mode { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? LogPath { get; init; }

    public string? RecoveryDirectory { get; init; }

    public InstalledStateManifest? InstalledState { get; init; }
}

public static class SetupFailureCodes
{
    public const string Cancelled = "cancelled";
    public const string OperationFailed = "operation-failed";
    public const string SafetyFailed = "safety-failed";
    public const string RecoveryFailed = "recovery-failed";
}
