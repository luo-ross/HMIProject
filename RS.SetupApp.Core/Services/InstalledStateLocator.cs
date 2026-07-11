namespace RS.SetupApp.Core;

public static class InstalledStateLocator
{
    public static InstalledStateManifest? TryLoad(
        ProductManifest product,
        InstallScope? requestedScope,
        ISystemPaths paths,
        IManifestSerializer serializer,
        IFileSystem fileSystem)
    {
        return TryLoad(
            product,
            requestedScope,
            paths,
            serializer,
            fileSystem,
            out _,
            out _);
    }

    public static InstalledStateManifest? TryLoad(
        ProductManifest product,
        InstallScope? requestedScope,
        ISystemPaths paths,
        IManifestSerializer serializer,
        IFileSystem fileSystem,
        out string? loadedManifestPath,
        out InstallScope? loadedScope)
    {
        loadedManifestPath = null;
        loadedScope = null;
        IEnumerable<InstallScope> scopes = requestedScope.HasValue
            ? [requestedScope.Value]
            : [InstallScope.CurrentUser, InstallScope.AllUsers];

        foreach (InstallScope scope in scopes)
        {
            string statePath = paths.GetStateManifestPath(product.ProductId, scope);
            if (fileSystem.FileExists(statePath))
            {
                loadedManifestPath = Path.GetFullPath(statePath);
                loadedScope = scope;
                return serializer.Load<InstalledStateManifest>(statePath);
            }
        }

        return null;
    }
}
