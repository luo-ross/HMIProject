namespace RS.SetupApp.Core;

public sealed class InvokeAfterUninstallExtensionsStep : ISetupStep
{
    public string Name => "Finalize uninstall extensions";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        foreach (ISetupExtension extension in context.Extensions)
        {
            await extension.AfterUninstallAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
