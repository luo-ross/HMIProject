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
        FakeDownloadService downloads)
    {
        PhysicalFileSystem fileSystem = new();
        JsonManifestSerializer serializer = new();
        InstallationOwnershipService ownershipService = new(fileSystem, serializer);
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
            PathSafetyPolicy = new SetupPathSafetyPolicy(fileSystem, ownershipService),
            OwnershipService = ownershipService,
            LoggerFactory = path => new FileSetupLogger(path)
        };
    }
}
