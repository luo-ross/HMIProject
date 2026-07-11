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
        SetupPathSafetyPolicy pathSafetyPolicy = new(fileSystem, ownershipService);
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
            PathSafetyPolicy = pathSafetyPolicy,
            OwnershipService = ownershipService,
            InstalledStateValidator = new InstalledStateValidator(
                fileSystem,
                paths,
                ownershipService,
                pathSafetyPolicy),
            LegacyInstallationClaimService = new LegacyInstallationClaimService(
                fileSystem,
                paths,
                serializer,
                ownershipService,
                pathSafetyPolicy),
            TransactionStore = new JsonSetupTransactionStore(fileSystem, serializer, paths),
            LoggerFactory = path => new FileSetupLogger(path)
        };
    }
}
