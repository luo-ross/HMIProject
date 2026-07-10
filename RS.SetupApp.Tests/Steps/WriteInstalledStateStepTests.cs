using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Steps;

[TestClass]
public sealed class WriteInstalledStateStepTests
{
    [TestMethod]
    public async Task ExecuteAsync_ShouldDeleteNewState_WhenMarkerWriteFailsOnFirstInstall()
    {
        using TempDirectoryScope temp = new();
        PhysicalFileSystem physical = new();
        FaultingFileSystem fileSystem = CreateMarkerWriteFailureFileSystem(physical);
        SetupExecutionContext context = CreateContext(temp, fileSystem, new JsonManifestSerializer());

        await Assert.ThrowsExceptionAsync<IOException>(
            () => new WriteInstalledStateStep().ExecuteAsync(context, CancellationToken.None));

        Assert.IsFalse(File.Exists(context.ResultState!.StateManifestPath));
        Assert.IsFalse(File.Exists(GetMarkerPath(context.ResultState.InstallDirectory)));
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldRestorePreviousState_WhenMarkerWriteFailsOnUpgrade()
    {
        using TempDirectoryScope temp = new();
        PhysicalFileSystem physical = new();
        JsonManifestSerializer serializer = new();
        FaultingFileSystem fileSystem = CreateMarkerWriteFailureFileSystem(physical);
        SetupExecutionContext context = CreateContext(temp, fileSystem, serializer);
        InstalledStateManifest previousState = CreateState(context.ResultState!, version: "1.0.0");
        serializer.Save(previousState.StateManifestPath, previousState);
        string previousContents = File.ReadAllText(previousState.StateManifestPath);

        await Assert.ThrowsExceptionAsync<IOException>(
            () => new WriteInstalledStateStep().ExecuteAsync(context, CancellationToken.None));

        Assert.IsTrue(File.Exists(previousState.StateManifestPath));
        Assert.AreEqual(previousContents, File.ReadAllText(previousState.StateManifestPath));
        Assert.IsFalse(File.Exists(GetMarkerPath(previousState.InstallDirectory)));
    }

    [TestMethod]
    public async Task RollbackAsync_ShouldRestorePreviousStateAndMarker_AfterSuccessfulUpgradeWrite()
    {
        using TempDirectoryScope temp = new();
        PhysicalFileSystem fileSystem = new();
        JsonManifestSerializer serializer = new();
        SetupExecutionContext context = CreateContext(temp, fileSystem, serializer);
        InstalledStateManifest previousState = CreateState(context.ResultState!, version: "1.0.0");
        serializer.Save(previousState.StateManifestPath, previousState);
        InstallationOwnershipMarker previousMarker = new()
        {
            ProductId = previousState.ProductId,
            InstallationId = previousState.InstallationId,
            InstallScope = previousState.InstallScope,
            CreatedAtUtc = previousState.InstalledAtUtc
        };
        context.Services.OwnershipService.Write(previousState.InstallDirectory, previousMarker);
        string previousStateContents = File.ReadAllText(previousState.StateManifestPath);
        string previousMarkerContents = File.ReadAllText(GetMarkerPath(previousState.InstallDirectory));
        WriteInstalledStateStep step = new();

        await step.ExecuteAsync(context, CancellationToken.None);
        await step.RollbackAsync(context, CancellationToken.None);

        Assert.IsTrue(File.Exists(previousState.StateManifestPath));
        Assert.IsTrue(File.Exists(GetMarkerPath(previousState.InstallDirectory)));
        Assert.AreEqual(previousStateContents, File.ReadAllText(previousState.StateManifestPath));
        Assert.AreEqual(previousMarkerContents, File.ReadAllText(GetMarkerPath(previousState.InstallDirectory)));
    }

    private static FaultingFileSystem CreateMarkerWriteFailureFileSystem(IFileSystem inner)
    {
        return new FaultingFileSystem(inner)
        {
            FailureFactory = (operation, path) =>
                operation == nameof(IFileSystem.WriteAllTextAtomic) &&
                string.Equals(Path.GetFileName(path), SetupRuntimeDefaults.OwnershipMarkerFileName, StringComparison.OrdinalIgnoreCase)
                    ? new IOException("Marker write failed.")
                    : null
        };
    }

    private static SetupExecutionContext CreateContext(
        TempDirectoryScope temp,
        IFileSystem fileSystem,
        IManifestSerializer serializer)
    {
        TestSystemPaths paths = new(temp.DirectoryPath);
        InstallationOwnershipService ownershipService = new(fileSystem, serializer);
        string installDirectory = Path.Combine(temp.DirectoryPath, "install", "demo-app");
        fileSystem.CreateDirectory(installDirectory);
        InstalledStateManifest state = new()
        {
            ProductId = "demo-app",
            InstallationId = Guid.Parse("9d2bd142-6a19-48de-8016-1484d17df69a"),
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = installDirectory,
            StateManifestPath = paths.GetStateManifestPath("demo-app", InstallScope.CurrentUser),
            Version = "2.0.0",
            InstalledAtUtc = new DateTimeOffset(2026, 7, 10, 1, 2, 3, TimeSpan.Zero)
        };

        return new SetupExecutionContext
        {
            Options = new RuntimeOptions(),
            Services = new SetupServices
            {
                FileSystem = fileSystem,
                Serializer = serializer,
                Registry = new FakeRegistryService(),
                Shortcuts = new FakeShortcutService(),
                Processes = new FakeProcessService(),
                Downloads = new FakeDownloadService(),
                Hasher = new DefaultFileHasher(),
                Paths = paths,
                PathSafetyPolicy = new SetupPathSafetyPolicy(fileSystem, ownershipService),
                OwnershipService = ownershipService,
                LoggerFactory = _ => new NullSetupLogger()
            },
            ProductManifestPath = Path.Combine(temp.DirectoryPath, "product.json"),
            PayloadDirectory = temp.DirectoryPath,
            ResultState = state
        };
    }

    private static InstalledStateManifest CreateState(InstalledStateManifest template, string version)
    {
        return new InstalledStateManifest
        {
            ProductId = template.ProductId,
            InstallationId = template.InstallationId,
            InstallScope = template.InstallScope,
            InstallDirectory = template.InstallDirectory,
            StateManifestPath = template.StateManifestPath,
            Version = version,
            InstalledAtUtc = template.InstalledAtUtc,
            LastSuccessfulInstallAtUtc = template.InstalledAtUtc
        };
    }

    private static string GetMarkerPath(string installDirectory)
    {
        return Path.Combine(installDirectory, SetupRuntimeDefaults.OwnershipMarkerFileName);
    }
}
