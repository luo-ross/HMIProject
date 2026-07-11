using RS.SetupApp.Core;

namespace RS.SetupApp.Services;

public interface ISetupRelaunchService
{
    Task<bool> TryRelaunchAsync(RuntimeOptions options, string[] arguments, CancellationToken cancellationToken);
}

public sealed class SetupRelaunchService : ISetupRelaunchService
{
    public async Task<bool> TryRelaunchAsync(RuntimeOptions options, string[] arguments, CancellationToken cancellationToken)
    {
        return await ElevationLauncher.TryRelaunchElevatedAsync(options, arguments, cancellationToken).ConfigureAwait(false) ||
               await SelfWorkerLauncher.TryRelaunchAsync(options, arguments, cancellationToken).ConfigureAwait(false);
    }
}

public sealed class NoopSetupRelaunchService : ISetupRelaunchService
{
    public Task<bool> TryRelaunchAsync(RuntimeOptions options, string[] arguments, CancellationToken cancellationToken) => Task.FromResult(false);
}
