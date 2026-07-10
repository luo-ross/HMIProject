using RS.SetupApp.Core;

namespace RS.SetupApp.Services;

public static class SetupServicesFactory
{
    public static SetupServices Create()
    {
        DefaultSystemPaths paths = new();
        PhysicalFileSystem fileSystem = new();
        JsonManifestSerializer serializer = new();
        InstallationOwnershipService ownershipService = new(fileSystem, serializer);
        return new SetupServices
        {
            FileSystem = fileSystem,
            Serializer = serializer,
            Registry = new WindowsRegistryService(),
            Shortcuts = new ShellShortcutService(paths),
            Processes = new ProcessService(),
            Downloads = new HttpDownloadService(),
            Hasher = new DefaultFileHasher(),
            Paths = paths,
            PathSafetyPolicy = new SetupPathSafetyPolicy(fileSystem, ownershipService),
            OwnershipService = ownershipService,
            LoggerFactory = path => new FileSetupLogger(path)
        };
    }
}
