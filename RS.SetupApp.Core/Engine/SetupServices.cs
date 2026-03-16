namespace RS.SetupApp.Core;

public sealed class SetupServices
{
    public required IFileSystem FileSystem { get; init; }

    public required IManifestSerializer Serializer { get; init; }

    public required IRegistryService Registry { get; init; }

    public required IShortcutService Shortcuts { get; init; }

    public required IProcessService Processes { get; init; }

    public required IDownloadService Downloads { get; init; }

    public required IFileHasher Hasher { get; init; }

    public required ISystemPaths Paths { get; init; }

    public required Func<string, ISetupLogger> LoggerFactory { get; init; }
}
