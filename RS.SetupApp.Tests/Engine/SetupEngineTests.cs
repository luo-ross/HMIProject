using RS.SetupApp.Builder;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Engine;

[TestClass]
public sealed class SetupEngineTests
{
    [TestMethod]
    public async Task ResolveAndValidate_ShouldThrowTypedScopeMismatch()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        string installDirectory = Path.Combine(temp.DirectoryPath, "empty-target");
        Directory.CreateDirectory(installDirectory);
        SetupExecutionContext context = new()
        {
            Options = new RuntimeOptions { Scope = InstallScope.CurrentUser },
            Services = TestSetupServicesFactory.Create(
                paths,
                new FakeRegistryService(),
                new FakeShortcutService(),
                new FakeProcessService(),
                new FakeDownloadService()),
            ProductManifestPath = Path.Combine(temp.DirectoryPath, "product.json"),
            PayloadDirectory = temp.DirectoryPath,
            Product = new ProductManifest
            {
                ProductId = "demo-app",
                InstallDefaults = new InstallDefaultsManifest { AllowOverwrite = true }
            },
            Package = new PackageManifest { Version = "2.0.0" },
            ExistingState = new InstalledStateManifest
            {
                ProductId = "demo-app",
                InstallationId = Guid.NewGuid(),
                InstallScope = InstallScope.AllUsers,
                InstallDirectory = installDirectory,
                Version = "1.0.0"
            }
        };

        await new ResolveOperationStateStep().ExecuteAsync(context, CancellationToken.None);
        SetupSafetyException exception = await Assert.ThrowsExceptionAsync<SetupSafetyException>(
            () => new ValidateInstallTargetStep().ExecuteAsync(context, CancellationToken.None));

        Assert.AreEqual(InstallScope.CurrentUser, context.EffectiveScope);
        Assert.AreEqual(InstallTargetFailureCode.ScopeMismatch, exception.FailureCode);
        Assert.AreEqual(InstallTargetFailureCode.ScopeMismatch, context.InstallTargetValidation?.FailureCode);
    }

    [TestMethod]
    public async Task ResolveAndValidate_ShouldPrioritizeTypedScopeMismatch_OverMachineInstallPolicy()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        string installDirectory = Path.Combine(temp.DirectoryPath, "empty-target");
        Directory.CreateDirectory(installDirectory);
        SetupExecutionContext context = new()
        {
            Options = new RuntimeOptions { Scope = InstallScope.AllUsers },
            Services = TestSetupServicesFactory.Create(
                paths,
                new FakeRegistryService(),
                new FakeShortcutService(),
                new FakeProcessService(),
                new FakeDownloadService()),
            ProductManifestPath = Path.Combine(temp.DirectoryPath, "product.json"),
            PayloadDirectory = temp.DirectoryPath,
            Product = new ProductManifest
            {
                ProductId = "demo-app",
                InstallDefaults = new InstallDefaultsManifest
                {
                    AllowMachineInstall = false,
                    AllowOverwrite = true
                }
            },
            Package = new PackageManifest { Version = "2.0.0" },
            ExistingState = new InstalledStateManifest
            {
                ProductId = "demo-app",
                InstallationId = Guid.NewGuid(),
                InstallScope = InstallScope.CurrentUser,
                InstallDirectory = installDirectory,
                Version = "1.0.0"
            }
        };

        await new ResolveOperationStateStep().ExecuteAsync(context, CancellationToken.None);
        SetupSafetyException exception = await Assert.ThrowsExceptionAsync<SetupSafetyException>(
            () => new ValidateInstallTargetStep().ExecuteAsync(context, CancellationToken.None));

        Assert.AreEqual(InstallTargetFailureCode.ScopeMismatch, exception.FailureCode);
        StringAssert.Contains(context.InstallTargetValidation?.Message, "installed-state scope");
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldInstallAndUninstallUsingOfflinePackage()
    {
        using TempDirectoryScope temp = new();
        string productDirectory = Path.Combine(temp.DirectoryPath, "product");
        Directory.CreateDirectory(productDirectory);
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, "demo-app", "DemoApp.exe");
        string publishDirectory = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoApp.exe", "1.0.0", "appsettings.json");
        string packageDirectory = await SetupTestDataFactory.CreatePackageAsync(
            publishDirectory,
            manifestPath,
            Path.Combine(temp.DirectoryPath, "packages"),
            packageVersion: "1.0.0").ConfigureAwait(false);

        JsonManifestSerializer serializer = new();
        PackageManifest package = serializer.Load<PackageManifest>(Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName));
        TestSystemPaths paths = new(temp.DirectoryPath);
        FakeRegistryService registry = new();
        FakeShortcutService shortcuts = new();
        FakeProcessService processes = new();
        FakeDownloadService downloads = new();
        SetupEngine engine = new(TestSetupServicesFactory.Create(paths, registry, shortcuts, processes, downloads));
        string installDirectory = paths.GetDefaultInstallDirectory(
            serializer.Load<ProductManifest>(manifestPath),
            InstallScope.CurrentUser);
        Directory.CreateDirectory(installDirectory);

        SetupOperationResult installResult = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Install,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath,
            PackageManifestPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName),
            PackagePath = Path.Combine(packageDirectory, package.ArchiveFileName),
            InstallDirectory = installDirectory
        }).ConfigureAwait(false);

        Assert.IsTrue(installResult.Succeeded);
        Assert.IsNotNull(installResult.InstalledState);
        Assert.IsTrue(File.Exists(installResult.InstalledState.MainExecutablePath));
        Assert.AreNotEqual(Guid.Empty, installResult.InstalledState.InstallationId);
        string ownershipMarkerPath = Path.Combine(
            installResult.InstalledState.InstallDirectory,
            SetupRuntimeDefaults.OwnershipMarkerFileName);
        Assert.IsTrue(File.Exists(ownershipMarkerPath));
        InstallationOwnershipMarker ownershipMarker = serializer.Load<InstallationOwnershipMarker>(ownershipMarkerPath);
        InstalledStateManifest persistedState = serializer.Load<InstalledStateManifest>(installResult.InstalledState.StateManifestPath);
        Assert.AreEqual(installResult.InstalledState.InstallationId, ownershipMarker.InstallationId);
        Assert.AreEqual(installResult.InstalledState.InstallationId, persistedState.InstallationId);
        Assert.AreEqual(installResult.InstalledState.ProductId, ownershipMarker.ProductId);
        Assert.AreEqual(installResult.InstalledState.InstallScope, ownershipMarker.InstallScope);
        Assert.AreEqual(1, registry.RegisterCallCount);
        Assert.AreEqual(1, shortcuts.CreateCallCount);
        CollectionAssert.Contains(processes.ClosedProcesses, "DemoApp");

        SetupOperationResult uninstallResult = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Uninstall,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath,
            PurgeData = true
        }).ConfigureAwait(false);

        Assert.IsTrue(uninstallResult.Succeeded);
        Assert.AreEqual(1, registry.RemoveCallCount);
        Assert.AreEqual(1, shortcuts.RemoveCallCount);
        Assert.IsFalse(Directory.Exists(installResult.InstalledState.InstallDirectory));
        Assert.IsFalse(File.Exists(installResult.InstalledState.StateManifestPath));
        foreach (string directoryPath in installResult.InstalledState.DataDirectories.Values)
        {
            Assert.IsFalse(Directory.Exists(directoryPath));
        }
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldUpdateFromLocalFeed()
    {
        using TempDirectoryScope temp = new();
        string productDirectory = Path.Combine(temp.DirectoryPath, "product");
        Directory.CreateDirectory(productDirectory);
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(
            productDirectory,
            "demo-app",
            "DemoApp.exe",
            allowOverwrite: true);

        string v1PublishDirectory = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoApp.exe", "1.0.0", "v1.txt");
        string v1PackageDirectory = await SetupTestDataFactory.CreatePackageAsync(
            v1PublishDirectory,
            manifestPath,
            Path.Combine(temp.DirectoryPath, "packages", "1.0.0"),
            packageVersion: "1.0.0").ConfigureAwait(false);

        JsonManifestSerializer serializer = new();
        ProductManifest product = serializer.Load<ProductManifest>(manifestPath);
        TestSystemPaths paths = new(temp.DirectoryPath);
        SetupEngine engine = new(TestSetupServicesFactory.Create(
            paths,
            new FakeRegistryService(),
            new FakeShortcutService(),
            new FakeProcessService(),
            new FakeDownloadService()));

        PackageManifest v1Package = serializer.Load<PackageManifest>(Path.Combine(v1PackageDirectory, SetupRuntimeDefaults.PackageManifestFileName));
        SetupOperationResult installResult = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Install,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath,
            PackageManifestPath = Path.Combine(v1PackageDirectory, SetupRuntimeDefaults.PackageManifestFileName),
            PackagePath = Path.Combine(v1PackageDirectory, v1Package.ArchiveFileName),
            InstallDirectory = paths.GetDefaultInstallDirectory(product, InstallScope.CurrentUser)
        }).ConfigureAwait(false);

        Assert.IsTrue(installResult.Succeeded);
        Assert.AreEqual("1.0.0", installResult.InstalledState?.Version);

        string v2PublishDirectory = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoApp.exe", "2.0.0", "v2.txt");
        string v2PackageDirectory = await SetupTestDataFactory.CreatePackageAsync(
            v2PublishDirectory,
            manifestPath,
            Path.Combine(temp.DirectoryPath, "packages", "2.0.0"),
            packageVersion: "2.0.0").ConfigureAwait(false);

        string updateFeedPath = new UpdateFeedPublisher(serializer).Publish(new BuilderOptions
        {
            Command = BuilderCommand.PublishUpdateFeed,
            PackageDirectory = v2PackageDirectory,
            BaseUrl = new Uri($"{v2PackageDirectory}{Path.DirectorySeparatorChar}", UriKind.Absolute).AbsoluteUri
        });

        product.Update.AllowOnlineUpdate = true;
        product.Update.ManifestUrl = updateFeedPath;
        serializer.Save(manifestPath, product);

        SetupOperationResult updateResult = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Update,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath,
            InstallDirectory = installResult.InstalledState?.InstallDirectory
        }).ConfigureAwait(false);

        Assert.IsTrue(updateResult.Succeeded);
        Assert.AreEqual("2.0.0", updateResult.InstalledState?.Version);
        Assert.AreEqual(installResult.InstalledState?.InstallationId, updateResult.InstalledState?.InstallationId);
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldReturnFriendlyMessage_WhenOfflinePayloadIsMissingAndOnlineUpdateIsDisabled()
    {
        using TempDirectoryScope temp = new();
        string productDirectory = Path.Combine(temp.DirectoryPath, "product");
        Directory.CreateDirectory(productDirectory);
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, "demo-app", "DemoApp.exe");

        JsonManifestSerializer serializer = new();
        ProductManifest product = serializer.Load<ProductManifest>(manifestPath);
        product.Update.AllowOnlineUpdate = false;
        serializer.Save(manifestPath, product);

        TestSystemPaths paths = new(temp.DirectoryPath);
        SetupEngine engine = new(TestSetupServicesFactory.Create(
            paths,
            new FakeRegistryService(),
            new FakeShortcutService(),
            new FakeProcessService(),
            new FakeDownloadService()));

        SetupOperationResult result = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Install,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath,
            InstallDirectory = paths.GetDefaultInstallDirectory(product, InstallScope.CurrentUser)
        }).ConfigureAwait(false);

        Assert.IsFalse(result.Succeeded);
        StringAssert.Contains(result.Message, "No offline package was found");
        StringAssert.Contains(result.Message, "installer shell");
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldReturnFriendlyMessage_WhenTemplateUpdateAddressIsStillConfigured()
    {
        using TempDirectoryScope temp = new();
        string productDirectory = Path.Combine(temp.DirectoryPath, "product");
        Directory.CreateDirectory(productDirectory);
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, "demo-app", "DemoApp.exe");

        JsonManifestSerializer serializer = new();
        ProductManifest product = serializer.Load<ProductManifest>(manifestPath);
        product.Update.AllowOnlineUpdate = true;
        product.Update.ManifestUrl = "https://example.com/downloads/latest.json";
        serializer.Save(manifestPath, product);

        TestSystemPaths paths = new(temp.DirectoryPath);
        SetupEngine engine = new(TestSetupServicesFactory.Create(
            paths,
            new FakeRegistryService(),
            new FakeShortcutService(),
            new FakeProcessService(),
            new FakeDownloadService()));

        SetupOperationResult result = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Install,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath,
            InstallDirectory = paths.GetDefaultInstallDirectory(product, InstallScope.CurrentUser)
        }).ConfigureAwait(false);

        Assert.IsFalse(result.Succeeded);
        StringAssert.Contains(result.Message, "template online update address");
        StringAssert.Contains(result.Message, "update.manifestUrl");
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldRecoverIncompleteTransactionBeforeLoadingState_ThenPermitInstall()
    {
        using TempDirectoryScope temp = new();
        string productDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "product")).FullName;
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, "demo-app", "DemoApp.exe");
        string publishDirectory = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoApp.exe", "1.0.0");
        string packageDirectory = await SetupTestDataFactory.CreatePackageAsync(
            publishDirectory,
            manifestPath,
            Path.Combine(temp.DirectoryPath, "packages"),
            packageVersion: "1.0.0");

        TestSystemPaths paths = new(temp.DirectoryPath);
        JsonManifestSerializer serializer = new();
        SetupServices services = TestSetupServicesFactory.Create(
            paths,
            new FakeRegistryService(),
            new FakeShortcutService(),
            new FakeProcessService(),
            new FakeDownloadService());
        Guid operationId = Guid.NewGuid();
        string recoveredTarget = Path.Combine(temp.DirectoryPath, "interrupted-mutation.txt");
        File.WriteAllText(recoveredTarget, "mutated");
        SetupTransactionJournal journal = CreateInterruptedJournal(paths, operationId, SetupTransactionPhase.Applying);
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = recoveredTarget,
            Applied = true
        });
        await services.TransactionStore.SaveAsync(journal, CancellationToken.None);

        PackageManifest package = serializer.Load<PackageManifest>(Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName));
        SetupOperationResult result = await new SetupEngine(services).ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Install,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath,
            PackageManifestPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName),
            PackagePath = Path.Combine(packageDirectory, package.ArchiveFileName),
            InstallDirectory = paths.GetDefaultInstallDirectory(serializer.Load<ProductManifest>(manifestPath), InstallScope.CurrentUser)
        });

        Assert.AreEqual(SetupOperationStatus.Succeeded, result.Status, result.Message);
        Assert.IsFalse(File.Exists(recoveredTarget));
        Assert.IsFalse(Directory.Exists(journal.RecoveryDirectory));
        Assert.IsTrue(File.Exists(result.InstalledState?.MainExecutablePath));
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldReturnStructuredFailureWhenProductManifestCannotBeFound()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        SetupEngine engine = new(TestSetupServicesFactory.Create(
            paths,
            new FakeRegistryService(),
            new FakeShortcutService(),
            new FakeProcessService(),
            new FakeDownloadService()));

        SetupOperationResult result = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Install,
            ProductManifestPath = Path.Combine(temp.DirectoryPath, "missing-product.json")
        });

        Assert.AreEqual(SetupOperationStatus.Failed, result.Status);
        Assert.AreEqual(SetupFailureCodes.OperationFailed, result.FailureCode);
        Assert.IsInstanceOfType<FileNotFoundException>(result.PrimaryError);
        Assert.IsFalse(result.Succeeded);
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldReturnRecoveryFailedAndBlockWorkUntilInterruptedTransactionRecovers()
    {
        using TempDirectoryScope temp = new();
        string productDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "product")).FullName;
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, "demo-app", "DemoApp.exe");
        string publishDirectory = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoApp.exe", "1.0.0");
        string packageDirectory = await SetupTestDataFactory.CreatePackageAsync(
            publishDirectory,
            manifestPath,
            Path.Combine(temp.DirectoryPath, "packages"),
            packageVersion: "1.0.0");

        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem physical = new();
        string recoveredTarget = Path.Combine(temp.DirectoryPath, "cannot-recover.txt");
        FaultInjectingFileSystem fileSystem = new(physical)
        {
            FailureFactory = (operation, path) => operation == nameof(IFileSystem.DeleteFile) &&
                string.Equals(path, recoveredTarget, StringComparison.OrdinalIgnoreCase)
                    ? new IOException("Recovery compensation failed.")
                    : null
        };
        JsonManifestSerializer serializer = new();
        ProductManifest product = serializer.Load<ProductManifest>(manifestPath);
        SetupServices services = TestSetupServicesFactory.Create(
            paths,
            new FakeRegistryService(),
            new FakeShortcutService(),
            new FakeProcessService(),
            new FakeDownloadService(),
            fileSystem);
        Guid operationId = Guid.NewGuid();
        File.WriteAllText(recoveredTarget, "evidence");
        SetupTransactionJournal journal = CreateInterruptedJournal(paths, operationId, SetupTransactionPhase.Applying);
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = recoveredTarget,
            Applied = true
        });
        await services.TransactionStore.SaveAsync(journal, CancellationToken.None);

        PackageManifest package = serializer.Load<PackageManifest>(Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName));
        RuntimeOptions options = new()
        {
            Mode = SetupMode.Install,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath,
            PackageManifestPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName),
            PackagePath = Path.Combine(packageDirectory, package.ArchiveFileName),
            InstallDirectory = paths.GetDefaultInstallDirectory(product, InstallScope.CurrentUser)
        };
        SetupEngine engine = new(services);

        SetupOperationResult blocked = await engine.ExecuteAsync(options);

        Assert.AreEqual(SetupOperationStatus.RecoveryFailed, blocked.Status);
        Assert.AreEqual(SetupFailureCodes.RecoveryFailed, blocked.FailureCode);
        Assert.AreEqual(operationId, blocked.OperationId);
        Assert.AreEqual(journal.RecoveryDirectory, blocked.RecoveryDirectory);
        Assert.IsTrue(blocked.RecoveryErrors.Count > 0);
        Assert.IsTrue(File.Exists(recoveredTarget));
        Assert.IsFalse(Directory.Exists(options.InstallDirectory!));

        fileSystem.FailureFactory = null;
        SetupOperationResult recovered = await engine.ExecuteAsync(options);

        Assert.AreEqual(SetupOperationStatus.Succeeded, recovered.Status, recovered.Message);
        Assert.IsFalse(File.Exists(recoveredTarget));
        Assert.IsTrue(File.Exists(recovered.InstalledState?.MainExecutablePath));
    }

    private static SetupTransactionJournal CreateInterruptedJournal(
        TestSystemPaths paths,
        Guid operationId,
        SetupTransactionPhase phase)
    {
        return new SetupTransactionJournal
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = InstallScope.CurrentUser,
            Mode = SetupMode.Install,
            InstallDirectory = Path.Combine(paths.RootPath, "install", InstallScope.CurrentUser.ToString(), "demo-app"),
            RecoveryDirectory = paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser),
            Phase = phase,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
