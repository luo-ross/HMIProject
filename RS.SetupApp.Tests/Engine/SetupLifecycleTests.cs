using RS.SetupApp.Builder;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;
using System.Text.Json;

namespace RS.SetupApp.Tests.Engine;

[TestClass]
public sealed class SetupLifecycleTests
{
    [TestMethod]
    public void FixtureManifest_ShouldBeAHarmlessCurrentUserTemplate()
    {
        string fixtureManifest = Path.Combine(
            FindRepositoryRoot(),
            "RS.SetupApp.Tests",
            "Fixtures",
            "TestPayloadApp",
            "fixture.product.json");

        Assert.IsTrue(File.Exists(fixtureManifest), "The disposable payload fixture manifest is required.");

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(fixtureManifest));
        JsonElement root = document.RootElement;
        Assert.AreEqual("CurrentUser", root.GetProperty("installDefaults").GetProperty("defaultScope").GetString());
        Assert.IsFalse(root.GetProperty("installDefaults").GetProperty("allowMachineInstall").GetBoolean());
        Assert.AreEqual(0, root.GetProperty("shortcuts").GetArrayLength());
        Assert.AreEqual(0, root.GetProperty("fileAssociations").GetArrayLength());
    }

    [TestMethod]
    public void LifecycleScripts_ShouldAvoidRestoreAfterTheCiRestoreStep()
    {
        string root = FindRepositoryRoot();
        string fixtureScript = File.ReadAllText(Path.Combine(root, "scripts", "Setup-SetupAppFixture.ps1"));
        string uiScript = File.ReadAllText(Path.Combine(root, "scripts", "Test-SetupAppUi.ps1"));

        StringAssert.Contains(fixtureScript, "'--no-restore'");
        StringAssert.Contains(uiScript, "--no-restore");
    }

    [TestMethod]
    public async Task InstallRepairCancelledUpdateUpdateHostileUninstallAndUninstall_ShouldKeepTheFixtureIsolated()
    {
        using TempDirectoryScope temp = new();
        string productId = $"setup-lifecycle-{Guid.NewGuid():N}";
        string productDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "product")).FullName;
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string productManifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, productId, "DemoApp.exe");
        JsonManifestSerializer serializer = new();
        ProductManifest product = serializer.Load<ProductManifest>(productManifestPath);
        product.Shortcuts.Clear();
        product.FileAssociations.Clear();
        product.InstallDefaults.AllowMachineInstall = false;
        product.InstallDefaults.AllowOverwrite = true;
        product.InstallDefaults.MinimumFreeSpaceBytes = 0;
        serializer.Save(productManifestPath, product);

        TestSystemPaths paths = new(temp.DirectoryPath);
        SetupEngine engine = new(TestSetupServicesFactory.Create(
            paths,
            new FakeRegistryService(),
            new FakeShortcutService(),
            new FakeProcessService(),
            new FakeDownloadService()));
        string installDirectory = Path.Combine(temp.DirectoryPath, "install", productId);

        string v1PackageDirectory = await CreatePackageAsync("1.0.0", "version-one").ConfigureAwait(false);
        PackageManifest v1Package = serializer.Load<PackageManifest>(Path.Combine(v1PackageDirectory, SetupRuntimeDefaults.PackageManifestFileName));
        RuntimeOptions v1Options = CreateOptions(SetupMode.Install, v1PackageDirectory, v1Package);

        SetupOperationResult install = await engine.ExecuteAsync(v1Options).ConfigureAwait(false);
        Assert.AreEqual(SetupOperationStatus.Succeeded, install.Status, install.Message);
        string installedExecutable = Path.Combine(installDirectory, "DemoApp.exe");
        byte[] v1Bytes = File.ReadAllBytes(installedExecutable);
        Assert.IsTrue(v1Bytes.SequenceEqual("version-one"u8.ToArray()));

        File.WriteAllText(Path.Combine(installDirectory, "payload.txt"), "tampered");
        SetupOperationResult repair = await engine.ExecuteAsync(CreateOptions(SetupMode.Repair, v1PackageDirectory, v1Package)).ConfigureAwait(false);
        Assert.AreEqual(SetupOperationStatus.Succeeded, repair.Status, repair.Message);
        Assert.AreEqual("payload-1.0.0", File.ReadAllText(Path.Combine(installDirectory, "payload.txt")));

        string v2PackageDirectory = await CreatePackageAsync("2.0.0", "version-two").ConfigureAwait(false);
        PackageManifest v2Package = serializer.Load<PackageManifest>(Path.Combine(v2PackageDirectory, SetupRuntimeDefaults.PackageManifestFileName));
        using CancellationTokenSource cancellation = new();
        IProgress<SetupProgress> cancelBeforeDeploy = new CallbackProgress(progress =>
        {
            if (progress.Message == "Deploy application files")
            {
                cancellation.Cancel();
            }
        });

        SetupOperationResult cancelledUpdate = await engine.ExecuteAsync(
            CreateOptions(SetupMode.Update, v2PackageDirectory, v2Package),
            cancelBeforeDeploy,
            cancellation.Token).ConfigureAwait(false);
        Assert.AreEqual(SetupOperationStatus.Cancelled, cancelledUpdate.Status, cancelledUpdate.Message);
        CollectionAssert.AreEqual(v1Bytes, File.ReadAllBytes(installedExecutable), "Cancelled update must restore the v1 executable bytes.");
        Assert.AreEqual("1.0.0", serializer.Load<InstalledStateManifest>(paths.GetStateManifestPath(productId, InstallScope.CurrentUser)).Version);

        SetupOperationResult update = await engine.ExecuteAsync(CreateOptions(SetupMode.Update, v2PackageDirectory, v2Package)).ConfigureAwait(false);
        Assert.AreEqual(SetupOperationStatus.Succeeded, update.Status, update.Message);
        CollectionAssert.AreEqual("version-two"u8.ToArray(), File.ReadAllBytes(installedExecutable));
        Assert.AreEqual("2.0.0", update.InstalledState?.Version);

        string statePath = paths.GetStateManifestPath(productId, InstallScope.CurrentUser);
        InstalledStateManifest validState = serializer.Load<InstalledStateManifest>(statePath);
        InstalledStateManifest hostileState = serializer.Load<InstalledStateManifest>(statePath);
        hostileState.ProductId = $"hostile-{Guid.NewGuid():N}";
        serializer.Save(statePath, hostileState);
        string sentinelPath = Path.Combine(temp.DirectoryPath, "outside-fixture-sentinel.txt");
        File.WriteAllText(sentinelPath, "do not change");

        SetupOperationResult refusedUninstall = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Uninstall,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = productManifestPath,
            InstallDirectory = installDirectory,
            PurgeData = true,
            SkipLaunch = true
        }).ConfigureAwait(false);
        Assert.AreEqual(SetupOperationStatus.Failed, refusedUninstall.Status);
        Assert.AreEqual("do not change", File.ReadAllText(sentinelPath));
        Assert.IsTrue(File.Exists(installedExecutable));

        serializer.Save(statePath, validState);
        SetupOperationResult uninstall = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Uninstall,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = productManifestPath,
            InstallDirectory = installDirectory,
            PurgeData = true,
            SkipLaunch = true
        }).ConfigureAwait(false);
        Assert.AreEqual(SetupOperationStatus.Succeeded, uninstall.Status, uninstall.Message);
        Assert.IsFalse(Directory.Exists(installDirectory));
        Assert.IsFalse(File.Exists(statePath));
        Assert.IsFalse(Directory.Exists(Path.GetDirectoryName(statePath)!));
        Assert.IsFalse(Directory.Exists(paths.GetRecoveryRoot(productId, InstallScope.CurrentUser)));

        async Task<string> CreatePackageAsync(string version, string executableContents)
        {
            string publishDirectory = SetupTestDataFactory.CreatePublishDirectory(
                temp.DirectoryPath,
                "DemoApp.exe",
                version,
                "payload.txt");
            File.WriteAllBytes(Path.Combine(publishDirectory, "DemoApp.exe"), System.Text.Encoding.UTF8.GetBytes(executableContents));
            File.WriteAllText(Path.Combine(publishDirectory, "payload.txt"), $"payload-{version}");
            return await SetupTestDataFactory.CreatePackageAsync(
                publishDirectory,
                productManifestPath,
                Path.Combine(temp.DirectoryPath, "packages", version),
                packageVersion: version).ConfigureAwait(false);
        }

        RuntimeOptions CreateOptions(SetupMode mode, string packageDirectory, PackageManifest package) => new()
        {
            Mode = mode,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = productManifestPath,
            PackageManifestPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName),
            PackagePath = Path.Combine(packageDirectory, package.ArchiveFileName),
            InstallDirectory = installDirectory,
            SkipLaunch = true
        };
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MultiVerseKit.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate the repository root.");
    }

    private sealed class CallbackProgress(Action<SetupProgress> callback) : IProgress<SetupProgress>
    {
        public void Report(SetupProgress value) => callback(value);
    }
}
