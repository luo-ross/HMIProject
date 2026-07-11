namespace RS.SetupApp.Core;

public sealed class CleanupWorkingDirectoryStep : ISetupStep
{
    public string Name => "Cleanup working directory";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(context.WorkingDirectory) && context.Services.FileSystem.DirectoryExists(context.WorkingDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(context.WorkingDirectory, recursive: true);
        }

        if (context.Journal?.Phase is SetupTransactionPhase.Committed or SetupTransactionPhase.RolledBack)
        {
            try
            {
                await context.Services.TransactionStore
                    .DeleteAsync(context.Journal, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                context.Logger?.Warn($"Unable to clean durable recovery data: {exception.Message}");
            }
        }
    }
}
