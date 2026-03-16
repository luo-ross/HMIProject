namespace RS.SetupApp.Core;

public sealed class ProductManifestLoadResult
{
    public string ProductManifestPath { get; init; } = string.Empty;

    public string SchemaPath { get; init; } = string.Empty;

    public ProductManifest? Manifest { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
