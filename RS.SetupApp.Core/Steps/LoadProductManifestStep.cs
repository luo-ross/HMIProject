namespace RS.SetupApp.Core;

public sealed class LoadProductManifestStep : ISetupStep
{
    public string Name => "Load product manifest";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.SchemaPath = ProductManifestLoader.ResolveSchemaPath(context.ProductManifestPath);
        context.Product = context.Services.Serializer.Load<ProductManifest>(context.ProductManifestPath);
        return Task.CompletedTask;
    }
}
