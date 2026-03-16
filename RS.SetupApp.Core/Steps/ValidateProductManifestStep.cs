namespace RS.SetupApp.Core;

public sealed class ValidateProductManifestStep : ISetupStep
{
    public string Name => "Validate product manifest";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        IReadOnlyList<string> errors = ProductManifestValidator.Validate(product, context.ProductManifestPath, context.Services.FileSystem.FileExists);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        }

        return Task.CompletedTask;
    }
}
