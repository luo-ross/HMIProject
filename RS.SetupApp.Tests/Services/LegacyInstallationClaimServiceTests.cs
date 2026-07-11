using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class LegacyInstallationClaimServiceTests
{
    [TestMethod]
    public async Task ClaimAsync_ShouldPersistOneInstallationIdToStateAndMarker()
    {
        using ClaimFixture fixture = new();

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsTrue(result.Succeeded, result.Message);
        Assert.IsTrue(result.Claimed);
        Assert.AreNotEqual(Guid.Empty, result.InstallationId);
        Assert.AreEqual(result.InstallationId, fixture.State.InstallationId);
        InstalledStateManifest persistedState = fixture.Serializer.Load<InstalledStateManifest>(fixture.StatePath);
        InstallationOwnershipMarker? marker = fixture.OwnershipService.Load(fixture.InstallDirectory);
        Assert.IsNotNull(marker);
        Assert.AreEqual(result.InstallationId, persistedState.InstallationId);
        Assert.AreEqual(result.InstallationId, marker.InstallationId);
        Assert.AreEqual(fixture.Product.ProductId, marker.ProductId);
        Assert.AreEqual(fixture.State.InstallScope, marker.InstallScope);
        Assert.AreEqual(1, fixture.FileSystem.Mutations.Count(mutation =>
            mutation.Operation == nameof(IFileSystem.WriteAllTextAtomic)));
        Assert.AreEqual(1, fixture.FileSystem.Mutations.Count(mutation =>
            mutation.Operation == nameof(IFileSystem.TryWriteAllTextNew)));
    }

    [TestMethod]
    public async Task ClaimAsync_ShouldBeIdempotent_WhenMatchingClaimAlreadyExists()
    {
        using ClaimFixture fixture = new();
        LegacyInstallationClaimResult first = await fixture.ClaimAsync();
        Assert.IsTrue(first.Succeeded, first.Message);
        fixture.FileSystem.Mutations.Clear();

        LegacyInstallationClaimResult second = await fixture.ClaimAsync();

        Assert.IsTrue(second.Succeeded, second.Message);
        Assert.IsFalse(second.Claimed);
        Assert.AreEqual(first.InstallationId, second.InstallationId);
        Assert.AreEqual(0, fixture.FileSystem.Mutations.Count);
    }

    [TestMethod]
    public async Task ClaimAsync_ShouldClaimSafeCustomInstallRoot()
    {
        using ClaimFixture fixture = new(useCustomInstallDirectory: true);

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsTrue(result.Succeeded, result.Message);
        Assert.IsTrue(result.Claimed);
        Assert.AreNotEqual(Guid.Empty, result.InstallationId);
    }

    [TestMethod]
    public async Task ClaimAsync_ShouldRejectCustomInstallRoot_WhenItIsAReparsePoint()
    {
        using ClaimFixture fixture = new(
            useCustomInstallDirectory: true,
            installRootIsReparsePoint: true);

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(Guid.Empty, fixture.State.InstallationId);
        Assert.AreEqual(0, fixture.FileSystem.Mutations.Count);
    }

    [DataTestMethod]
    [DataRow("product")]
    [DataRow("scope")]
    [DataRow("version")]
    [DataRow("main-executable-path")]
    [DataRow("install-path")]
    [DataRow("missing-main-executable")]
    [DataRow("package-product")]
    [DataRow("package-version")]
    [DataRow("missing-package-manifest")]
    public async Task ClaimAsync_ShouldPerformZeroWrites_WhenLegacyEvidenceDoesNotMatch(string tamper)
    {
        using ClaimFixture fixture = new();
        switch (tamper)
        {
            case "product":
                fixture.State.ProductId = "another-product";
                break;
            case "scope":
                fixture.State.InstallScope = InstallScope.AllUsers;
                break;
            case "version":
                fixture.State.Version = string.Empty;
                break;
            case "main-executable-path":
                fixture.State.MainExecutablePath = Path.Combine(fixture.Temp.DirectoryPath, "sibling", "DemoApp.exe");
                break;
            case "install-path":
                fixture.State.InstallDirectory = Path.Combine(fixture.Temp.DirectoryPath, "sibling-install");
                break;
            case "missing-main-executable":
                File.Delete(fixture.State.MainExecutablePath);
                break;
            case "package-product":
                fixture.WritePackageManifest("another-product", fixture.State.Version);
                break;
            case "package-version":
                fixture.WritePackageManifest(fixture.Product.ProductId, "9.9.9");
                break;
            case "missing-package-manifest":
                File.Delete(fixture.PackageManifestPath);
                break;
        }

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsFalse(result.Succeeded);
        Assert.IsFalse(result.Claimed);
        Assert.AreEqual(Guid.Empty, fixture.State.InstallationId);
        Assert.AreEqual(0, fixture.FileSystem.Mutations.Count);
        Assert.IsNull(fixture.OwnershipService.Load(fixture.InstallDirectory));
    }

    [TestMethod]
    public async Task ClaimAsync_ShouldPerformZeroWrites_WhenMarkerConflicts()
    {
        using ClaimFixture fixture = new();
        fixture.OwnershipService.Write(fixture.InstallDirectory, new InstallationOwnershipMarker
        {
            ProductId = fixture.Product.ProductId,
            InstallationId = Guid.NewGuid(),
            InstallScope = fixture.State.InstallScope
        });
        fixture.FileSystem.Mutations.Clear();

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsFalse(result.Succeeded);
        Assert.IsFalse(result.Claimed);
        Assert.AreEqual(Guid.Empty, fixture.State.InstallationId);
        Assert.AreEqual(0, fixture.FileSystem.Mutations.Count);
    }

    [TestMethod]
    public async Task ClaimAsync_ShouldRestoreExternalState_WhenMarkerWriteFails()
    {
        using ClaimFixture fixture = new();
        string markerPath = Path.Combine(
            fixture.InstallDirectory,
            SetupRuntimeDefaults.OwnershipMarkerFileName);
        fixture.FileSystem.FailureFactory = (operation, path) =>
            operation == nameof(IFileSystem.TryWriteAllTextNew) && PathsEqual(path, markerPath)
                ? new IOException("marker write failed")
                : null;

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(Guid.Empty, fixture.State.InstallationId);
        Assert.AreEqual(Guid.Empty, fixture.Serializer.Load<InstalledStateManifest>(fixture.StatePath).InstallationId);
        Assert.IsNull(fixture.OwnershipService.Load(fixture.InstallDirectory));
    }

    [TestMethod]
    public async Task ClaimAsync_ShouldPreserveForeignMarkerAndState_WhenMarkerAppearsDuringWrite()
    {
        using ClaimFixture fixture = new();
        string originalState = File.ReadAllText(fixture.StatePath);
        string markerPath = Path.Combine(
            fixture.InstallDirectory,
            SetupRuntimeDefaults.OwnershipMarkerFileName);
        InstallationOwnershipMarker foreignMarker = new()
        {
            ProductId = "foreign-product",
            InstallationId = Guid.NewGuid(),
            InstallScope = InstallScope.CurrentUser,
            CreatedAtUtc = new DateTimeOffset(2026, 7, 11, 1, 2, 3, TimeSpan.Zero)
        };
        fixture.FileSystem.FailureFactory = (operation, path) =>
        {
            if (operation != nameof(IFileSystem.TryWriteAllTextNew) || !PathsEqual(path, markerPath))
            {
                return null;
            }

            fixture.Serializer.Save(markerPath, foreignMarker);
            return new IOException("foreign marker won the race");
        };

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(originalState, File.ReadAllText(fixture.StatePath));
        InstallationOwnershipMarker? persistedMarker = fixture.OwnershipService.Load(fixture.InstallDirectory);
        Assert.IsNotNull(persistedMarker);
        Assert.AreEqual(foreignMarker.ProductId, persistedMarker.ProductId);
        Assert.AreEqual(foreignMarker.InstallationId, persistedMarker.InstallationId);
        Assert.AreEqual(foreignMarker.InstallScope, persistedMarker.InstallScope);
        Assert.AreEqual(foreignMarker.CreatedAtUtc, persistedMarker.CreatedAtUtc);
    }

    [TestMethod]
    public async Task ClaimAsync_ShouldPerformZeroWrites_WhenClaimLockTimesOut()
    {
        FakeLegacyInstallationClaimLockProvider lockProvider = new()
        {
            ShouldTimeout = true
        };
        using ClaimFixture fixture = new(lockProvider: lockProvider);
        string originalState = File.ReadAllText(fixture.StatePath);

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("claim-lock-timeout", result.FailureCode);
        Assert.AreEqual(originalState, File.ReadAllText(fixture.StatePath));
        Assert.AreEqual(0, fixture.FileSystem.Mutations.Count);
        Assert.IsNull(fixture.OwnershipService.Load(fixture.InstallDirectory));
    }

    [TestMethod]
    public async Task ClaimAsync_ShouldRejectForeignMarker_InjectedAfterClaimLockAcquisition()
    {
        FakeLegacyInstallationClaimLockProvider lockProvider = new();
        using ClaimFixture fixture = new(lockProvider: lockProvider);
        string originalState = File.ReadAllText(fixture.StatePath);
        InstallationOwnershipMarker foreignMarker = new()
        {
            ProductId = "foreign-product",
            InstallationId = Guid.NewGuid(),
            InstallScope = InstallScope.CurrentUser
        };
        lockProvider.OnAcquired = () => fixture.Serializer.Save(
            Path.Combine(fixture.InstallDirectory, SetupRuntimeDefaults.OwnershipMarkerFileName),
            foreignMarker);

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("ownership-marker-conflict", result.FailureCode);
        Assert.AreEqual(originalState, File.ReadAllText(fixture.StatePath));
        Assert.AreEqual(0, fixture.FileSystem.Mutations.Count);
        InstallationOwnershipMarker? persistedMarker = fixture.OwnershipService.Load(fixture.InstallDirectory);
        Assert.IsNotNull(persistedMarker);
        Assert.AreEqual(foreignMarker.ProductId, persistedMarker.ProductId);
        Assert.AreEqual(foreignMarker.InstallationId, persistedMarker.InstallationId);
    }

    [TestMethod]
    public async Task ClaimAsync_ShouldNotClobberForeignMarker_WhenItAppearsImmediatelyBeforeCreation()
    {
        using ClaimFixture fixture = new();
        string originalState = File.ReadAllText(fixture.StatePath);
        string markerPath = Path.Combine(
            fixture.InstallDirectory,
            SetupRuntimeDefaults.OwnershipMarkerFileName);
        InstallationOwnershipMarker foreignMarker = new()
        {
            ProductId = "foreign-product",
            InstallationId = Guid.NewGuid(),
            InstallScope = InstallScope.CurrentUser
        };
        bool injected = false;
        fixture.FileSystem.FailureFactory = (operation, path) =>
        {
            bool isMarkerCreation = operation == nameof(IFileSystem.WriteAllTextAtomic) ||
                operation == "TryWriteAllTextNew";
            if (injected || !isMarkerCreation || !PathsEqual(path, markerPath))
            {
                return null;
            }

            injected = true;
            fixture.Serializer.Save(markerPath, foreignMarker);
            return null;
        };

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("ownership-marker-conflict", result.FailureCode);
        Assert.AreEqual(originalState, File.ReadAllText(fixture.StatePath));
        InstallationOwnershipMarker? persistedMarker = fixture.OwnershipService.Load(fixture.InstallDirectory);
        Assert.IsNotNull(persistedMarker);
        Assert.AreEqual(foreignMarker.ProductId, persistedMarker.ProductId);
        Assert.AreEqual(foreignMarker.InstallationId, persistedMarker.InstallationId);
    }

    [TestMethod]
    public async Task ClaimAsync_ShouldDeleteOnlyOwnMarker_WhenStateWriteFailsAfterMarkerCreation()
    {
        using ClaimFixture fixture = new();
        string originalState = File.ReadAllText(fixture.StatePath);
        string markerPath = Path.Combine(
            fixture.InstallDirectory,
            SetupRuntimeDefaults.OwnershipMarkerFileName);
        fixture.FileSystem.FailureFactory = (operation, path) =>
            operation == nameof(IFileSystem.WriteAllTextAtomic) && PathsEqual(path, fixture.StatePath)
                ? new IOException("state write failed")
                : null;

        LegacyInstallationClaimResult result = await fixture.ClaimAsync();

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(originalState, File.ReadAllText(fixture.StatePath));
        Assert.IsFalse(File.Exists(markerPath));
        Assert.AreEqual(1, fixture.FileSystem.Mutations.Count(mutation =>
            mutation.Operation == "TryWriteAllTextNew" && PathsEqual(mutation.Path, markerPath)));
        Assert.AreEqual(1, fixture.FileSystem.Mutations.Count(mutation =>
            mutation.Operation == nameof(IFileSystem.DeleteFile) && PathsEqual(mutation.Path, markerPath)));
    }

    private static bool PathsEqual(string left, string right)
    {
        return string.Equals(
            Path.GetFullPath(left),
            Path.GetFullPath(right),
            StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ClaimFixture : IDisposable
    {
        public ClaimFixture(
            bool useCustomInstallDirectory = false,
            bool installRootIsReparsePoint = false,
            ILegacyInstallationClaimLockProvider? lockProvider = null)
        {
            Temp = new TempDirectoryScope();
            Paths = new TestSystemPaths(Temp.DirectoryPath);
            Serializer = new JsonManifestSerializer();
            FileSystem = new FaultingFileSystem(new PhysicalFileSystem());
            OwnershipService = new InstallationOwnershipService(FileSystem, Serializer);
            Product = new ProductManifest
            {
                ProductId = "demo-app",
                DisplayName = "Demo App",
                Publisher = "Contoso",
                MainExecutable = "DemoApp.exe",
                InstallDefaults = new InstallDefaultsManifest
                {
                    DefaultScope = InstallScope.CurrentUser,
                    AllowOverwrite = true
                }
            };
            InstallDirectory = useCustomInstallDirectory
                ? Path.Combine(Temp.DirectoryPath, "custom-install", Product.ProductId)
                : Paths.GetDefaultInstallDirectory(Product, InstallScope.CurrentUser);
            if (installRootIsReparsePoint)
            {
                string realInstallDirectory = Directory.CreateDirectory(
                    Path.Combine(Temp.DirectoryPath, "real-install")).FullName;
                Directory.CreateDirectory(Path.GetDirectoryName(InstallDirectory)!);
                Directory.CreateSymbolicLink(InstallDirectory, realInstallDirectory);
            }
            else
            {
                Directory.CreateDirectory(InstallDirectory);
            }
            File.WriteAllText(Path.Combine(InstallDirectory, Product.MainExecutable), "legacy executable");

            string maintenanceDirectory = Paths.GetMaintenanceDirectory(InstallDirectory);
            PackageManifestPath = Path.Combine(
                maintenanceDirectory,
                SetupRuntimeDefaults.DefaultPayloadFolderName,
                SetupRuntimeDefaults.PackageManifestFileName);
            WritePackageManifest(Product.ProductId, "1.0.0");

            StatePath = Paths.GetStateManifestPath(Product.ProductId, InstallScope.CurrentUser);
            State = new InstalledStateManifest
            {
                ProductId = Product.ProductId,
                InstallationId = Guid.Empty,
                Version = "1.0.0",
                InstallScope = InstallScope.CurrentUser,
                InstallDirectory = InstallDirectory,
                MainExecutablePath = Path.Combine(InstallDirectory, Product.MainExecutable),
                StateManifestPath = StatePath,
                MaintenanceDirectory = maintenanceDirectory,
                MaintenanceExecutablePath = Path.Combine(
                    maintenanceDirectory,
                    Path.GetFileName(Environment.ProcessPath ?? "Setup.exe")),
                MaintenanceProductManifestPath = Path.Combine(
                    maintenanceDirectory,
                    SetupRuntimeDefaults.DefaultPayloadFolderName,
                    SetupRuntimeDefaults.ProductManifestFileName),
                MaintenancePackageManifestPath = PackageManifestPath,
                AutorunEntryName = SetupPathUtility.SanitizePathSegment(Product.ProductId)
            };
            Serializer.Save(StatePath, State);
            FileSystem.Mutations.Clear();

            SetupPathSafetyPolicy pathSafetyPolicy = new(FileSystem, OwnershipService);
            Service = new LegacyInstallationClaimService(
                FileSystem,
                Paths,
                Serializer,
                OwnershipService,
                pathSafetyPolicy,
                lockProvider,
                TimeSpan.FromMilliseconds(50));
        }

        public TempDirectoryScope Temp { get; }

        public TestSystemPaths Paths { get; }

        public JsonManifestSerializer Serializer { get; }

        public FaultingFileSystem FileSystem { get; }

        public InstallationOwnershipService OwnershipService { get; }

        public LegacyInstallationClaimService Service { get; }

        public ProductManifest Product { get; }

        public InstalledStateManifest State { get; }

        public string InstallDirectory { get; }

        public string StatePath { get; }

        public string PackageManifestPath { get; }

        public Task<LegacyInstallationClaimResult> ClaimAsync()
        {
            return Service.ClaimAsync(Product, State, CancellationToken.None);
        }

        public void WritePackageManifest(string productId, string version)
        {
            Serializer.Save(PackageManifestPath, new PackageManifest
            {
                ProductId = productId,
                Version = version,
                MainExecutable = Product.MainExecutable
            });
        }

        public void Dispose() => Temp.Dispose();
    }

    private sealed class FakeLegacyInstallationClaimLockProvider : ILegacyInstallationClaimLockProvider
    {
        public bool ShouldTimeout { get; init; }

        public Action? OnAcquired { get; set; }

        public IDisposable? TryAcquire(
            string productId,
            string canonicalInstallRoot,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (ShouldTimeout)
            {
                return null;
            }

            OnAcquired?.Invoke();
            return new Releaser();
        }

        private sealed class Releaser : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
