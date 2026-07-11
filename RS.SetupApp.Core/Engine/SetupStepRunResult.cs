namespace RS.SetupApp.Core;

public sealed class SetupStepRunResult
{
    public required bool Completed { get; init; }

    public Exception? PrimaryError { get; init; }

    public IReadOnlyList<string> RecoveryErrors { get; init; } = Array.Empty<string>();
}
