using System.Security.Cryptography;
using System.Text;

namespace RS.SetupApp.Core;

public sealed class NamedLegacyInstallationClaimLockProvider : ILegacyInstallationClaimLockProvider
{
    public IDisposable? TryAcquire(
        string productId,
        string canonicalInstallRoot,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string mutexName = CreateMutexName(productId, canonicalInstallRoot);
        Mutex mutex = new(initiallyOwned: false, mutexName);
        bool acquired = false;

        try
        {
            int signaledIndex = WaitHandle.WaitAny(
                [mutex, cancellationToken.WaitHandle],
                NormalizeTimeout(timeout));
            if (signaledIndex == 1)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            if (signaledIndex == WaitHandle.WaitTimeout)
            {
                return null;
            }

            acquired = true;
            return new MutexLease(mutex);
        }
        catch (AbandonedMutexException)
        {
            acquired = true;
            return new MutexLease(mutex);
        }
        finally
        {
            if (!acquired)
            {
                mutex.Dispose();
            }
        }
    }

    private static string CreateMutexName(string productId, string canonicalInstallRoot)
    {
        string identity = string.Concat(
            productId.Trim().ToUpperInvariant(),
            "\n",
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(canonicalInstallRoot)).ToUpperInvariant());
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
        return $"RS.SetupApp.LegacyClaim.{Convert.ToHexString(hash)}";
    }

    private static TimeSpan NormalizeTimeout(TimeSpan timeout)
    {
        if (timeout == Timeout.InfiniteTimeSpan)
        {
            return timeout;
        }

        if (timeout < TimeSpan.Zero || timeout.TotalMilliseconds > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        return timeout;
    }

    private sealed class MutexLease(Mutex mutex) : IDisposable
    {
        private Mutex? _mutex = mutex;

        public void Dispose()
        {
            Mutex? ownedMutex = Interlocked.Exchange(ref _mutex, null);
            if (ownedMutex == null)
            {
                return;
            }

            try
            {
                ownedMutex.ReleaseMutex();
            }
            finally
            {
                ownedMutex.Dispose();
            }
        }
    }
}
