namespace RS.SetupApp.Core;

public sealed class InvokeAfterInstallExtensionsStep : ISetupStep
{
    public string Name => "Finalize install extensions";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        foreach (ISetupExtension extension in context.Extensions)
        {
            await extension.AfterInstallAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
