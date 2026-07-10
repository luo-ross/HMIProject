using RS.SetupApp.Builder;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Engine;

[TestClass]
public sealed class SetupEngineTests
{
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
}
