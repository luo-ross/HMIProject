namespace RS.SetupApp.Core;

public sealed class ProductManifestLocationResult
{
    public ProductManifestLocationResult(string resolvedPath, IReadOnlyList<string> searchedPaths, bool exists)
    {
        ResolvedPath = resolvedPath;
        SearchedPaths = searchedPaths;
        Exists = exists;
    }

    public bool Exists { get; }

    public string ResolvedPath { get; }

    public IReadOnlyList<string> SearchedPaths { get; }
}
