namespace RS.SetupApp.Core;

public interface ILegacyInstallationClaimLockProvider
{
    IDisposable? TryAcquire(
        string productId,
        string canonicalInstallRoot,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}
