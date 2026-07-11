namespace RS.SetupApp.Core;

public sealed record InstalledStateValidationResult(
    UninstallPlan? Plan,
    string? FailureCode,
    string Message)
{
    public bool IsValid => Plan is not null;

    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
