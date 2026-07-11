namespace RS.SetupApp.Core;

public sealed class InstallationOwnershipService
{
    private readonly IFileSystem _fileSystem;
    private readonly IManifestSerializer _serializer;

    public InstallationOwnershipService(IFileSystem fileSystem, IManifestSerializer serializer)
    {
        _fileSystem = fileSystem;
        _serializer = serializer;
    }

    public InstallationOwnershipMarker? Load(string installDirectory)
    {
        string markerPath = GetMarkerPath(installDirectory);
        return _fileSystem.FileExists(markerPath)
            ? _serializer.Load<InstallationOwnershipMarker>(markerPath)
            : null;
    }

    public void Write(string installDirectory, InstallationOwnershipMarker marker)
    {
        InstallationOwnershipMarker? existing = Load(installDirectory);
        if (existing != null &&
            existing.SchemaVersion == marker.SchemaVersion &&
            string.Equals(existing.ProductId, marker.ProductId, StringComparison.OrdinalIgnoreCase) &&
            existing.InstallationId == marker.InstallationId &&
            existing.InstallScope == marker.InstallScope)
        {
            return;
        }

        string markerPath = GetMarkerPath(installDirectory);
        _fileSystem.WriteAllTextAtomic(markerPath, _serializer.Serialize(marker));
    }

    public bool TryCreate(string installDirectory, InstallationOwnershipMarker marker)
    {
        string markerPath = GetMarkerPath(installDirectory);
        return _fileSystem.TryWriteAllTextNew(markerPath, _serializer.Serialize(marker));
    }

    public void Delete(string installDirectory)
    {
        _fileSystem.DeleteFile(GetMarkerPath(installDirectory));
    }

    private static string GetMarkerPath(string installDirectory)
    {
        return Path.Combine(installDirectory, SetupRuntimeDefaults.OwnershipMarkerFileName);
    }
}
