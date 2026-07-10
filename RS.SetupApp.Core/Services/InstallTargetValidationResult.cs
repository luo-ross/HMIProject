namespace RS.SetupApp.Core;

public sealed record InstallTargetValidationResult(
    bool IsValid,
    string? NormalizedPath,
    InstallTargetFailureCode FailureCode,
    string Message);
