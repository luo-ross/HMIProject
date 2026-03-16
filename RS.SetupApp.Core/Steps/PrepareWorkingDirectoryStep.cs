namespace RS.SetupApp.Core;

public sealed class PrepareWorkingDirectoryStep : ISetupStep, IRollbackStep
{
    public string Name => "Prepare working directory";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        context.WorkingDirectory ??= context.Services.Paths.GetTemporaryWorkingDirectory(product.ProductId);
        context.Services.FileSystem.CreateDirectory(context.WorkingDirectory);
        return Task.CompletedTask;
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.IsNullOrWhiteSpace(context.WorkingDirectory) && context.Services.FileSystem.DirectoryExists(context.WorkingDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(context.WorkingDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
