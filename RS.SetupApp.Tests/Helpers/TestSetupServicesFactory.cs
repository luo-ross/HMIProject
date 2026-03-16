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
        return new SetupServices
        {
            FileSystem = new PhysicalFileSystem(),
            Serializer = new JsonManifestSerializer(),
            Registry = registry,
            Shortcuts = shortcuts,
            Processes = processes,
            Downloads = downloads,
            Hasher = new DefaultFileHasher(),
            Paths = paths,
            LoggerFactory = path => new FileSetupLogger(path)
        };
    }
}
