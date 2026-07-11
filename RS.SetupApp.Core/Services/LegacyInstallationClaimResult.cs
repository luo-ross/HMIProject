namespace RS.SetupApp.Core;

public sealed record LegacyInstallationClaimResult(
    bool Succeeded,
    bool Claimed,
    Guid InstallationId,
    string? FailureCode,
    string Message);
