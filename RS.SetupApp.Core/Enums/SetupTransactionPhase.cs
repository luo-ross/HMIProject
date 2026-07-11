namespace RS.SetupApp.Core;

public enum SetupTransactionPhase
{
    Prepared,
    SnapshotCreated,
    Applying,
    Verifying,
    Committing,
    Committed,
    RollingBack,
    RolledBack,
    RecoveryFailed
}
