using RS.SetupApp.Core;

namespace RS.SetupApp.Services;

public static class SetupServicesFactory
{
    public static SetupServices Create()
    {
        DefaultSystemPaths paths = new();
        return new SetupServices
        {
            FileSystem = new PhysicalFileSystem(),
            Serializer = new JsonManifestSerializer(),
            Registry = new WindowsRegistryService(),
            Shortcuts = new ShellShortcutService(paths),
            Processes = new ProcessService(),
            Downloads = new HttpDownloadService(),
            Hasher = new DefaultFileHasher(),
            Paths = paths,
            LoggerFactory = path => new FileSetupLogger(path)
        };
    }
}
