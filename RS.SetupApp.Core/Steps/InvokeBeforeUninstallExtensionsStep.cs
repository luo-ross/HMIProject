namespace RS.SetupApp.Core;

public sealed class InvokeBeforeUninstallExtensionsStep : ISetupStep
{
    public string Name => "Run uninstall extensions";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        foreach (ISetupExtension extension in context.Extensions)
        {
            await extension.BeforeUninstallAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
