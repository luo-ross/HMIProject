namespace RS.SetupApp.Core;

public sealed class WriteInstalledStateStep : ISetupStep, IRollbackStep
{
    public string Name => "Write installed state";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");
        state.LastSuccessfulInstallAtUtc = DateTimeOffset.UtcNow;
        context.Services.Serializer.Save(state.StateManifestPath, state);
        return Task.CompletedTask;
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");
        context.Services.FileSystem.DeleteFile(state.StateManifestPath);
        return Task.CompletedTask;
    }
}
