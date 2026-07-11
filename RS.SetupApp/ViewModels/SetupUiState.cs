namespace RS.SetupApp.ViewModels;

public enum SetupUiState
{
    Idle,
    Preparing,
    Running,
    CancellationRequested,
    RollingBack,
    Succeeded,
    Failed,
    RecoveryFailed
}
