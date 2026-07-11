namespace RS.SetupApp.Core;

public sealed record SetupRecoveryResult(
    bool Succeeded,
    SetupTransactionJournal Journal,
    IReadOnlyList<string> Errors)
{
    public IReadOnlyList<string> CleanupWarnings { get; init; } = [];
}
