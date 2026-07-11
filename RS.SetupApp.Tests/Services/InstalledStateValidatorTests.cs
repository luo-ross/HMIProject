using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class InstalledStateValidatorTests
{
    [TestMethod]
    public void Validate_ShouldReturnCanonicalImmutablePlan_ForMatchingOwnedState()
    {
        using ValidatorFixture fixture = new();

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsTrue(result.IsValid, result.Message);
        Assert.IsNotNull(result.Plan);
        Assert.AreEqual(fixture.InstallDirectory, result.Plan.InstallDirectory);
        Assert.AreEqual(fixture.State.StateManifestPath, result.Plan.StateManifestPath);
        Assert.AreEqual(2, result.Plan.FileSystemTargets.Count);
        Assert.IsTrue(result.Plan.FileSystemTargets.Any(target =>
            target.Purpose == SetupPathPurpose.InstallRoot && PathsEqual(target.Path, fixture.InstallDirectory)));
        Assert.IsTrue(result.Plan.FileSystemTargets.Any(target =>
            target.Purpose == SetupPathPurpose.StateManifest && PathsEqual(target.Path, fixture.State.StateManifestPath)));
        Assert.AreEqual(1, result.Plan.Shortcuts.Count);
        Assert.AreNotSame(fixture.State.Shortcuts, result.Plan.Shortcuts);
        Assert.AreEqual(fixture.Paths.GetShortcutPath(
            fixture.Product,
            fixture.Product.Shortcuts[0],
            fixture.State.InstallScope), result.Plan.Shortcuts[0].Path);
    }

    [TestMethod]
    public void Validate_ShouldIncludeOnlyCanonicalPurgeDataTargets_WhenPurgeIsEnabled()
    {
        using ValidatorFixture fixture = new();
        fixture.Options.PurgeData = true;

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsTrue(result.IsValid, result.Message);
        Assert.IsNotNull(result.Plan);
        UninstallTarget dataTarget = result.Plan.FileSystemTargets.Single(target =>
            target.Purpose == SetupPathPurpose.DataRoot);
        Assert.AreEqual(fixture.State.DataDirectories["userData"], dataTarget.Path);
    }

    [TestMethod]
    public void Validate_ShouldAcceptExplicitCanonicalCustomInstallRoot_WhenOwnershipMatches()
    {
        using ValidatorFixture fixture = new(useCustomInstallDirectory: true);

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsTrue(result.IsValid, result.Message);
        Assert.IsNotNull(result.Plan);
        Assert.AreEqual(fixture.InstallDirectory, result.Plan.InstallDirectory);
    }

    [TestMethod]
    public void Validate_ShouldAcceptOwnedCustomInstallRoot_WithoutExplicitInstallDirectory()
    {
        using ValidatorFixture fixture = new(
            useCustomInstallDirectory: true,
            provideExplicitInstallDirectory: false);

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsTrue(result.IsValid, result.Message);
        Assert.IsNotNull(result.Plan);
        Assert.AreEqual(fixture.InstallDirectory, result.Plan.InstallDirectory);
    }

    [TestMethod]
    public void Validate_ShouldRejectExplicitInstallDirectory_WhenItDoesNotMatchOwnedState()
    {
        using ValidatorFixture fixture = new();
        fixture.Options.InstallDirectory = Path.Combine(fixture.Temp.DirectoryPath, "different-install");

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("install-path-mismatch", result.FailureCode);
    }

    [TestMethod]
    public void Validate_ShouldWarnAndExcludeLegacyBackupOutsideRecoveryRoot()
    {
        using ValidatorFixture fixture = new();
        string sentinelDirectory = Directory.CreateDirectory(
            Path.Combine(fixture.Temp.DirectoryPath, "sibling-backup")).FullName;
        fixture.State.PendingBackupDirectory = sentinelDirectory;
        fixture.State.LastBackupDirectory = Path.Combine(
            fixture.Paths.GetRecoveryRoot(fixture.Product.ProductId, fixture.State.InstallScope),
            "..",
            "escaped-backup");

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsTrue(result.IsValid, result.Message);
        Assert.IsNotNull(result.Plan);
        Assert.AreEqual(2, result.Warnings.Count);
        Assert.IsFalse(result.Plan.FileSystemTargets.Any(target => target.Purpose == SetupPathPurpose.BackupRoot));
        Assert.IsTrue(Directory.Exists(sentinelDirectory));
    }

    [TestMethod]
    public void Validate_ShouldIncludeDistinctLegacyBackupsBelowRecoveryRoot()
    {
        using ValidatorFixture fixture = new();
        string backupDirectory = Directory.CreateDirectory(Path.Combine(
            fixture.Paths.GetRecoveryRoot(fixture.Product.ProductId, fixture.State.InstallScope),
            Guid.NewGuid().ToString("N"))).FullName;
        fixture.State.PendingBackupDirectory = backupDirectory;
        fixture.State.LastBackupDirectory = backupDirectory;

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsTrue(result.IsValid, result.Message);
        Assert.IsNotNull(result.Plan);
        Assert.AreEqual(1, result.Plan.FileSystemTargets.Count(target => target.Purpose == SetupPathPurpose.BackupRoot));
    }

    [TestMethod]
    public void Validate_ShouldFailClosed_WhenRegisteredFileAssociationIsMissing()
    {
        using ValidatorFixture fixture = new();
        fixture.Product.FileAssociations.Add(new FileAssociationManifest
        {
            Extension = ".demo",
            ProgId = "Contoso.Demo",
            FriendlyName = "Demo file"
        });

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("registry-state-mismatch", result.FailureCode);
    }

    [DataTestMethod]
    [DataRow("product")]
    [DataRow("scope")]
    [DataRow("installation-id")]
    [DataRow("marker-product")]
    [DataRow("marker-scope")]
    [DataRow("marker-installation-id")]
    [DataRow("marker-schema")]
    public void Validate_ShouldFailClosed_OnIdentityOrOwnershipMismatch(string tamper)
    {
        using ValidatorFixture fixture = new();
        switch (tamper)
        {
            case "product":
                fixture.State.ProductId = "another-product";
                break;
            case "scope":
                fixture.State.InstallScope = InstallScope.AllUsers;
                break;
            case "installation-id":
                fixture.State.InstallationId = Guid.Empty;
                break;
            case "marker-product":
                fixture.RewriteMarker("another-product", fixture.State.InstallationId, fixture.State.InstallScope);
                break;
            case "marker-scope":
                fixture.RewriteMarker(fixture.Product.ProductId, fixture.State.InstallationId, InstallScope.AllUsers);
                break;
            case "marker-installation-id":
                fixture.RewriteMarker(fixture.Product.ProductId, Guid.NewGuid(), fixture.State.InstallScope);
                break;
            case "marker-schema":
                fixture.RewriteMarker(
                    fixture.Product.ProductId,
                    fixture.State.InstallationId,
                    fixture.State.InstallScope,
                    schemaVersion: 2);
                break;
        }

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsFalse(result.IsValid);
        Assert.IsNull(result.Plan);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.FailureCode));
    }

    [DataTestMethod]
    [DataRow("install-sibling")]
    [DataRow("main-parent")]
    [DataRow("maintenance-special-root")]
    [DataRow("state-sibling")]
    [DataRow("data-reparse")]
    [DataRow("shortcut-parent")]
    public void Validate_ShouldRejectTamperedDestructivePaths(string tamper)
    {
        using ValidatorFixture fixture = new();
        switch (tamper)
        {
            case "install-sibling":
                fixture.State.InstallDirectory = Path.Combine(fixture.Temp.DirectoryPath, "sibling-install");
                break;
            case "main-parent":
                fixture.State.MainExecutablePath = Path.Combine(fixture.InstallDirectory, "..", "sentinel.exe");
                break;
            case "maintenance-special-root":
                fixture.State.MaintenanceDirectory = Path.GetPathRoot(fixture.Temp.DirectoryPath)!;
                break;
            case "state-sibling":
                fixture.State.StateManifestPath = Path.Combine(fixture.Temp.DirectoryPath, "sibling-state.json");
                break;
            case "data-reparse":
                string realDirectory = Directory.CreateDirectory(Path.Combine(fixture.Temp.DirectoryPath, "real-data")).FullName;
                string linkedDirectory = Path.Combine(fixture.Temp.DirectoryPath, "linked-data");
                Directory.CreateSymbolicLink(linkedDirectory, realDirectory);
                fixture.State.DataDirectories["userData"] = linkedDirectory;
                break;
            case "shortcut-parent":
                fixture.State.Shortcuts[0].Path = Path.Combine(fixture.Temp.DirectoryPath, "..", "sentinel.lnk");
                break;
        }

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsFalse(result.IsValid);
        Assert.IsNull(result.Plan);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.FailureCode));
    }

    [TestMethod]
    public void Validate_ShouldRejectCanonicalInstallRoot_WhenItIsAReparsePoint()
    {
        using ValidatorFixture fixture = new(createInstallDirectory: false);
        string realDirectory = Directory.CreateDirectory(Path.Combine(fixture.Temp.DirectoryPath, "real-install")).FullName;
        Directory.CreateDirectory(Path.GetDirectoryName(fixture.InstallDirectory)!);
        Directory.CreateSymbolicLink(fixture.InstallDirectory, realDirectory);
        fixture.RewriteMarker(fixture.Product.ProductId, fixture.State.InstallationId, fixture.State.InstallScope);

        InstalledStateValidationResult result = fixture.Validate();

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("unsafe-install-path", result.FailureCode);
    }

    private static bool PathsEqual(string left, string right)
    {
        return string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ValidatorFixture : IDisposable
    {
        private readonly PhysicalFileSystem _fileSystem = new();
        private readonly JsonManifestSerializer _serializer = new();
        private readonly InstallationOwnershipService _ownershipService;
        private readonly InstalledStateValidator _validator;

        public ValidatorFixture(
            bool createInstallDirectory = true,
            bool useCustomInstallDirectory = false,
            bool provideExplicitInstallDirectory = true)
        {
            Temp = new TempDirectoryScope();
            Paths = new TestSystemPaths(Temp.DirectoryPath);
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
                },
                Uninstall = new UninstallPolicyManifest { AllowPurgeData = true },
                Shortcuts =
                [
                    new ShortcutManifest
                    {
                        Name = "Demo App",
                        Location = ShortcutLocation.Desktop,
                        EnabledByDefault = true
                    }
                ],
                DataDirectories =
                [
                    new DataDirectoryManifest
                    {
                        Key = "userData",
                        Scope = DataDirectoryScope.UserLocal,
                        RelativePath = Path.Combine("Contoso", "demo-app")
                    }
                ]
            };
            Options = new RuntimeOptions
            {
                Mode = SetupMode.Uninstall,
                Scope = InstallScope.CurrentUser
            };
            InstallDirectory = useCustomInstallDirectory
                ? Path.Combine(Temp.DirectoryPath, "custom-install", Product.ProductId)
                : Paths.GetDefaultInstallDirectory(Product, InstallScope.CurrentUser);
            if (useCustomInstallDirectory && provideExplicitInstallDirectory)
            {
                Options.InstallDirectory = InstallDirectory;
            }
            if (createInstallDirectory)
            {
                Directory.CreateDirectory(InstallDirectory);
            }

            Guid installationId = Guid.NewGuid();
            string maintenanceDirectory = Paths.GetMaintenanceDirectory(InstallDirectory);
            State = new InstalledStateManifest
            {
                ProductId = Product.ProductId,
                InstallationId = installationId,
                Version = "1.0.0",
                InstallScope = InstallScope.CurrentUser,
                InstallDirectory = InstallDirectory,
                MainExecutablePath = Path.Combine(InstallDirectory, Product.MainExecutable),
                StateManifestPath = Paths.GetStateManifestPath(Product.ProductId, InstallScope.CurrentUser),
                MaintenanceDirectory = maintenanceDirectory,
                MaintenanceExecutablePath = Path.Combine(
                    maintenanceDirectory,
                    Path.GetFileName(Environment.ProcessPath ?? "Setup.exe")),
                MaintenanceProductManifestPath = Path.Combine(
                    maintenanceDirectory,
                    SetupRuntimeDefaults.DefaultPayloadFolderName,
                    SetupRuntimeDefaults.ProductManifestFileName),
                MaintenancePackageManifestPath = Path.Combine(
                    maintenanceDirectory,
                    SetupRuntimeDefaults.DefaultPayloadFolderName,
                    SetupRuntimeDefaults.PackageManifestFileName),
                AutorunEntryName = SetupPathUtility.SanitizePathSegment(Product.ProductId),
                Shortcuts =
                [
                    new RegisteredShortcutState
                    {
                        Name = "Demo App",
                        Location = ShortcutLocation.Desktop,
                        Path = Paths.GetShortcutPath(Product, Product.Shortcuts[0], InstallScope.CurrentUser)
                    }
                ],
                DataDirectories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["userData"] = Paths.GetDataDirectory(Product, InstallScope.CurrentUser, Product.DataDirectories[0])
                }
            };

            _ownershipService = new InstallationOwnershipService(_fileSystem, _serializer);
            if (createInstallDirectory)
            {
                RewriteMarker(Product.ProductId, installationId, InstallScope.CurrentUser);
            }

            SetupPathSafetyPolicy pathSafetyPolicy = new(_fileSystem, _ownershipService);
            _validator = new InstalledStateValidator(_fileSystem, Paths, _ownershipService, pathSafetyPolicy);
        }

        public TempDirectoryScope Temp { get; }

        public TestSystemPaths Paths { get; }

        public ProductManifest Product { get; }

        public RuntimeOptions Options { get; }

        public string InstallDirectory { get; }

        public InstalledStateManifest State { get; }

        public InstalledStateValidationResult Validate() => _validator.Validate(Product, State, Options);

        public void RewriteMarker(
            string productId,
            Guid installationId,
            InstallScope scope,
            int schemaVersion = 1)
        {
            string markerPath = Path.Combine(InstallDirectory, SetupRuntimeDefaults.OwnershipMarkerFileName);
            Directory.CreateDirectory(InstallDirectory);
            if (File.Exists(markerPath))
            {
                File.Delete(markerPath);
            }

            _ownershipService.Write(InstallDirectory, new InstallationOwnershipMarker
            {
                SchemaVersion = schemaVersion,
                ProductId = productId,
                InstallationId = installationId,
                InstallScope = scope
            });
        }

        public void Dispose() => Temp.Dispose();
    }
}
