using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;

namespace RS.SetupApp.Tests.Helpers;

public static class TestSetupServicesFactory
{
    public static SetupServices Create(
        TestSystemPaths paths,
        FakeRegistryService registry,
        FakeShortcutService shortcuts,
        FakeProcessService processes,
        FakeDownloadService downloads,
        IFileSystem? fileSystem = null)
    {
        fileSystem ??= new PhysicalFileSystem();
        shortcuts.Paths = paths;
        JsonManifestSerializer serializer = new();
        InstallationOwnershipService ownershipService = new(fileSystem, serializer);
        SetupPathSafetyPolicy pathSafetyPolicy = new(fileSystem, ownershipService);
        return new SetupServices
        {
            FileSystem = fileSystem,
            Serializer = serializer,
            Registry = registry,
            Shortcuts = shortcuts,
            Processes = processes,
            Downloads = downloads,
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
            LoggerFactory = path => new FileSetupLogger(path)
        };
    }
}
