using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Engine;

[TestClass]
public sealed class UninstallSafetyTests
{
    [DataTestMethod]
    [DataRow("product")]
    [DataRow("scope")]
    [DataRow("installation-id")]
    [DataRow("marker")]
    [DataRow("install-sibling")]
    [DataRow("main-parent")]
    [DataRow("main-reparse")]
    [DataRow("maintenance-special-root")]
    [DataRow("state-sibling")]
    [DataRow("data-sibling")]
    [DataRow("shortcut-parent")]
    public async Task ExecuteAsync_ShouldPerformZeroMutations_WhenInstalledStateIsTampered(string tamper)
    {
        using UninstallFixture fixture = new();
        string protectedSentinel = fixture.CreateTamper(tamper);
        fixture.PersistState();
        fixture.FileSystem.Mutations.Clear();

        SetupOperationResult result = await fixture.ExecuteUninstallAsync();

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(0, fixture.FileSystem.Mutations.Count,
            string.Join(Environment.NewLine, fixture.FileSystem.Mutations.Select(mutation =>
                $"{mutation.Operation}: {mutation.Path}")));
        Assert.AreEqual(0, fixture.Registry.RemoveCallCount);
        Assert.AreEqual(0, fixture.Shortcuts.RemoveCallCount);
        Assert.AreEqual(0, fixture.Processes.ClosedProcesses.Count);
        Assert.IsTrue(File.Exists(protectedSentinel), $"Sentinel '{protectedSentinel}' was mutated.");
        Assert.IsTrue(File.Exists(fixture.LoadedStatePath));
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldIgnoreLegacyBackupsOutsideRecoveryRoot()
    {
        using UninstallFixture fixture = new();
        string backupDirectory = Directory.CreateDirectory(
            Path.Combine(fixture.Temp.DirectoryPath, "sibling-backup")).FullName;
        string sentinel = Path.Combine(backupDirectory, "sentinel.txt");
        File.WriteAllText(sentinel, "do not delete");
        fixture.State.PendingBackupDirectory = backupDirectory;
        fixture.State.LastBackupDirectory = Path.Combine(
            fixture.Paths.GetRecoveryRoot(fixture.Product.ProductId, InstallScope.CurrentUser),
            "..",
            "escaped-backup");
        fixture.PersistState();
        fixture.FileSystem.Mutations.Clear();

        SetupOperationResult result = await fixture.ExecuteUninstallAsync();

        Assert.IsTrue(result.Succeeded, result.Message);
        Assert.IsTrue(File.Exists(sentinel));
        Assert.IsFalse(fixture.FileSystem.Mutations.Any(mutation => PathsEqual(mutation.Path, backupDirectory)));
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldFailWithoutMutation_ForUnclaimedLegacyInstall()
    {
        using UninstallFixture fixture = new(writeMarker: false, installationId: Guid.Empty);
        fixture.FileSystem.Mutations.Clear();

        SetupOperationResult result = await fixture.ExecuteUninstallAsync(claimLegacy: false);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(0, fixture.FileSystem.Mutations.Count);
        Assert.IsTrue(File.Exists(fixture.State.MainExecutablePath));
        Assert.IsTrue(File.Exists(fixture.LoadedStatePath));
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldClaimOnlyWhenExplicitAndThenUseClaimedPlan()
    {
        using UninstallFixture fixture = new(writeMarker: false, installationId: Guid.Empty);
        fixture.FileSystem.Mutations.Clear();

        SetupOperationResult result = await fixture.ExecuteUninstallAsync(claimLegacy: true);

        Assert.IsTrue(result.Succeeded, result.Message);
        Assert.AreEqual(1, fixture.Registry.RemoveCallCount);
        Assert.IsTrue(fixture.FileSystem.Mutations.Count(mutation =>
            mutation.Operation == nameof(IFileSystem.WriteAllTextAtomic)) >= 2);
        Assert.IsFalse(File.Exists(fixture.LoadedStatePath));
        Assert.IsFalse(Directory.Exists(fixture.InstallDirectory));
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldNotWriteClaim_WhenExplicitInstallDirectoryMismatchesStateRoot()
    {
        using UninstallFixture fixture = new(writeMarker: false, installationId: Guid.Empty);
        string originalState = File.ReadAllText(fixture.LoadedStatePath);
        string markerPath = Path.Combine(
            fixture.InstallDirectory,
            SetupRuntimeDefaults.OwnershipMarkerFileName);
        fixture.FileSystem.Mutations.Clear();

        SetupOperationResult result = await fixture.ExecuteUninstallAsync(
            claimLegacy: true,
            installDirectory: Path.Combine(fixture.Temp.DirectoryPath, "different-install"));

        Assert.IsFalse(result.Succeeded);
        StringAssert.Contains(result.Message, "requested-install-path-mismatch");
        Assert.AreEqual(originalState, File.ReadAllText(fixture.LoadedStatePath));
        Assert.IsFalse(File.Exists(markerPath));
        Assert.AreEqual(0, fixture.FileSystem.Mutations.Count(mutation =>
            mutation.Operation == nameof(IFileSystem.WriteAllTextAtomic)));
    }

    private static bool PathsEqual(string left, string right)
    {
        try
        {
            return string.Equals(
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private sealed class UninstallFixture : IDisposable
    {
        private readonly JsonManifestSerializer _serializer = new();
        private readonly InstallationOwnershipService _ownershipService;
        private readonly SetupEngine _engine;

        public UninstallFixture(bool writeMarker = true, Guid? installationId = null)
        {
            Temp = new TempDirectoryScope();
            string productDirectory = Directory.CreateDirectory(
                Path.Combine(Temp.DirectoryPath, "product")).FullName;
            SetupTestDataFactory.WriteProductSchema(productDirectory);
            ProductManifestPath = SetupTestDataFactory.WriteProductManifest(
                productDirectory,
                "demo-app",
                "DemoApp.exe",
                allowOverwrite: true);
            Product = _serializer.Load<ProductManifest>(ProductManifestPath);
            Paths = new TestSystemPaths(Temp.DirectoryPath);
            FileSystem = new FaultingFileSystem(new PhysicalFileSystem());
            Registry = new FakeRegistryService();
            Shortcuts = new FakeShortcutService();
            Processes = new FakeProcessService();
            _ownershipService = new InstallationOwnershipService(FileSystem, _serializer);

            InstallDirectory = Paths.GetDefaultInstallDirectory(Product, InstallScope.CurrentUser);
            Directory.CreateDirectory(InstallDirectory);
            string mainExecutablePath = Path.Combine(InstallDirectory, Product.MainExecutable);
            File.WriteAllText(mainExecutablePath, "installed executable");

            string maintenanceDirectory = Paths.GetMaintenanceDirectory(InstallDirectory);
            Directory.CreateDirectory(maintenanceDirectory);
            string packageManifestPath = Path.Combine(
                maintenanceDirectory,
                SetupRuntimeDefaults.DefaultPayloadFolderName,
                SetupRuntimeDefaults.PackageManifestFileName);
            _serializer.Save(packageManifestPath, new PackageManifest
            {
                ProductId = Product.ProductId,
                Version = "1.0.0",
                MainExecutable = Product.MainExecutable
            });

            Guid stateInstallationId = installationId ?? Guid.NewGuid();
            LoadedStatePath = Paths.GetStateManifestPath(Product.ProductId, InstallScope.CurrentUser);
            State = new InstalledStateManifest
            {
                ProductId = Product.ProductId,
                InstallationId = stateInstallationId,
                DisplayName = Product.DisplayName,
                Publisher = Product.Publisher,
                Version = "1.0.0",
                InstallScope = InstallScope.CurrentUser,
                InstallDirectory = InstallDirectory,
                MainExecutablePath = mainExecutablePath,
                StateManifestPath = LoadedStatePath,
                MaintenanceDirectory = maintenanceDirectory,
                MaintenanceExecutablePath = Path.Combine(
                    maintenanceDirectory,
                    Path.GetFileName(Environment.ProcessPath ?? "Setup.exe")),
                MaintenanceProductManifestPath = Path.Combine(
                    maintenanceDirectory,
                    SetupRuntimeDefaults.DefaultPayloadFolderName,
                    SetupRuntimeDefaults.ProductManifestFileName),
                MaintenancePackageManifestPath = packageManifestPath,
                AutorunEntryName = SetupPathUtility.SanitizePathSegment(Product.ProductId),
                Shortcuts = Product.Shortcuts.Take(1).Select(shortcut => new RegisteredShortcutState
                {
                    Name = string.IsNullOrWhiteSpace(shortcut.Name) ? Product.DisplayName : shortcut.Name,
                    Location = shortcut.Location,
                    Path = Paths.GetShortcutPath(Product, shortcut, InstallScope.CurrentUser)
                }).ToList(),
                DataDirectories = Product.DataDirectories.ToDictionary(
                    directory => directory.Key,
                    directory => Paths.GetDataDirectory(Product, InstallScope.CurrentUser, directory),
                    StringComparer.OrdinalIgnoreCase)
            };

            foreach (string dataDirectory in State.DataDirectories.Values)
            {
                Directory.CreateDirectory(dataDirectory);
                File.WriteAllText(Path.Combine(dataDirectory, "data-sentinel.txt"), "product data");
            }

            PersistState();
            if (writeMarker)
            {
                RewriteMarker(stateInstallationId);
            }

            SetupServices services = TestSetupServicesFactory.Create(
                Paths,
                Registry,
                Shortcuts,
                Processes,
                new FakeDownloadService(),
                FileSystem);
            _engine = new SetupEngine(services);
            FileSystem.Mutations.Clear();
        }

        public TempDirectoryScope Temp { get; }

        public TestSystemPaths Paths { get; }

        public FaultingFileSystem FileSystem { get; }

        public FakeRegistryService Registry { get; }

        public FakeShortcutService Shortcuts { get; }

        public FakeProcessService Processes { get; }

        public ProductManifest Product { get; }

        public InstalledStateManifest State { get; }

        public string ProductManifestPath { get; }

        public string InstallDirectory { get; }

        public string LoadedStatePath { get; }

        public string CreateTamper(string tamper)
        {
            string protectedSentinel = Path.Combine(InstallDirectory, "protected-sentinel.txt");
            File.WriteAllText(protectedSentinel, "do not mutate");
            switch (tamper)
            {
                case "product":
                    State.ProductId = "another-product";
                    break;
                case "scope":
                    State.InstallScope = InstallScope.AllUsers;
                    break;
                case "installation-id":
                    State.InstallationId = Guid.Empty;
                    break;
                case "marker":
                    RewriteMarker(Guid.NewGuid());
                    break;
                case "install-sibling":
                    string siblingInstall = Directory.CreateDirectory(
                        Path.Combine(Temp.DirectoryPath, "sibling-install")).FullName;
                    protectedSentinel = Path.Combine(siblingInstall, "sentinel.txt");
                    File.WriteAllText(protectedSentinel, "do not mutate");
                    State.InstallDirectory = siblingInstall;
                    break;
                case "main-parent":
                    State.MainExecutablePath = Path.Combine(InstallDirectory, "..", "sentinel.exe");
                    break;
                case "main-reparse":
                    string realExecutable = Path.Combine(Temp.DirectoryPath, "real-sentinel.exe");
                    File.WriteAllText(realExecutable, "do not mutate");
                    string linkedExecutable = Path.Combine(Temp.DirectoryPath, "linked-sentinel.exe");
                    File.CreateSymbolicLink(linkedExecutable, realExecutable);
                    State.MainExecutablePath = linkedExecutable;
                    protectedSentinel = realExecutable;
                    break;
                case "maintenance-special-root":
                    State.MaintenanceDirectory = Path.GetPathRoot(Temp.DirectoryPath)!;
                    break;
                case "state-sibling":
                    string siblingState = Path.Combine(Temp.DirectoryPath, "sibling-state.json");
                    File.WriteAllText(siblingState, "do not mutate");
                    State.StateManifestPath = siblingState;
                    protectedSentinel = siblingState;
                    break;
                case "data-sibling":
                    string siblingData = Directory.CreateDirectory(
                        Path.Combine(Temp.DirectoryPath, "sibling-data")).FullName;
                    protectedSentinel = Path.Combine(siblingData, "sentinel.txt");
                    File.WriteAllText(protectedSentinel, "do not mutate");
                    State.DataDirectories["userData"] = siblingData;
                    break;
                case "shortcut-parent":
                    State.Shortcuts[0].Path = Path.Combine(Temp.DirectoryPath, "..", "sentinel.lnk");
                    break;
            }

            return protectedSentinel;
        }

        public void PersistState() => _serializer.Save(LoadedStatePath, State);

        public Task<SetupOperationResult> ExecuteUninstallAsync(
            bool claimLegacy = false,
            string? installDirectory = null)
        {
            return _engine.ExecuteAsync(new RuntimeOptions
            {
                Mode = SetupMode.Uninstall,
                Scope = InstallScope.CurrentUser,
                ProductManifestPath = ProductManifestPath,
                InstallDirectory = installDirectory,
                PurgeData = true,
                ClaimLegacyInstallation = claimLegacy
            });
        }

        public void Dispose() => Temp.Dispose();

        private void RewriteMarker(Guid installationId)
        {
            string markerPath = Path.Combine(InstallDirectory, SetupRuntimeDefaults.OwnershipMarkerFileName);
            if (File.Exists(markerPath))
            {
                File.Delete(markerPath);
            }

            _ownershipService.Write(InstallDirectory, new InstallationOwnershipMarker
            {
                ProductId = Product.ProductId,
                InstallationId = installationId,
                InstallScope = InstallScope.CurrentUser
            });
        }
    }
}
