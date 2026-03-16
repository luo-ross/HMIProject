namespace RS.SetupApp.Core;

public sealed class RemoveDataDirectoriesStep : ISetupStep
{
    public string Name => "Remove product data";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        InstalledStateManifest state = context.ExistingState ?? throw new InvalidOperationException("Installed state has not been loaded.");
        bool purgeData = product.Uninstall.AllowPurgeData && (context.Options.PurgeData || product.Uninstall.PurgeDataByDefault);
        if (!purgeData)
        {
            return Task.CompletedTask;
        }

        foreach (string directoryPath in state.DataDirectories.Values.Where(context.Services.FileSystem.DirectoryExists))
        {
            context.Services.FileSystem.DeleteDirectory(directoryPath, recursive: true);
        }

        return Task.CompletedTask;
    }
}
