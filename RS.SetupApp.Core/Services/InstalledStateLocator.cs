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
        IEnumerable<InstallScope> scopes = requestedScope.HasValue
            ? [requestedScope.Value]
            : [InstallScope.CurrentUser, InstallScope.AllUsers];

        foreach (InstallScope scope in scopes)
        {
            string statePath = paths.GetStateManifestPath(product.ProductId, scope);
            if (fileSystem.FileExists(statePath))
            {
                return serializer.Load<InstalledStateManifest>(statePath);
            }
        }

        return null;
    }
}
