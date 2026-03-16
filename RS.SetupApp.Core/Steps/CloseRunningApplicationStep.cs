namespace RS.SetupApp.Core;

public sealed class CloseRunningApplicationStep : ISetupStep
{
    public string Name => "Close running application";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        return context.Services.Processes.CloseAsync(product.GetMainProcessName(), cancellationToken);
    }
}
