using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Engine;

[TestClass]
public sealed class SetupCancellationIntegrationTests
{
    [TestMethod]
    public void GetSilentExitCode_ShouldMapStructuredOperationStatuses()
    {
        Assert.AreEqual(0, RuntimeArgumentParser.GetSilentExitCode(new SetupOperationResult
        {
            Status = SetupOperationStatus.Succeeded
        }));
        Assert.AreEqual(2, RuntimeArgumentParser.GetSilentExitCode(new SetupOperationResult
        {
            Status = SetupOperationStatus.Cancelled
        }));
        Assert.AreEqual(3, RuntimeArgumentParser.GetSilentExitCode(new SetupOperationResult
        {
            Status = SetupOperationStatus.Failed
        }));
        Assert.AreEqual(4, RuntimeArgumentParser.GetSilentExitCode(new SetupOperationResult
        {
            Status = SetupOperationStatus.RecoveryFailed
        }));
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldReturnCancelledOnlyAfterInstallRecoveryRestoresTheOriginalTree()
    {
        using TempDirectoryScope temp = new();
        string productDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "product")).FullName;
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, "demo-app", "DemoApp.exe");
        string publishDirectory = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoApp.exe", "1.0.0");
        string packageDirectory = await SetupTestDataFactory.CreatePackageAsync(publishDirectory, manifestPath, Path.Combine(temp.DirectoryPath, "packages"));
        JsonManifestSerializer serializer = new();
        ProductManifest product = serializer.Load<ProductManifest>(manifestPath);
        PackageManifest package = serializer.Load<PackageManifest>(Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName));
        TestSystemPaths paths = new(temp.DirectoryPath);
        SetupEngine engine = new(CreateServices(paths));
        using CancellationTokenSource cancellation = new();

        SetupOperationResult result = await engine.ExecuteAsync(
            CreateInstallOptions(paths, product, manifestPath, packageDirectory, package),
            new InlineProgress(step =>
            {
                if (step.Message == "Deploy maintenance runtime")
                {
                    cancellation.Cancel();
                }
            }),
            cancellation.Token);

        Assert.AreEqual(SetupOperationStatus.Cancelled, result.Status);
        Assert.AreEqual(SetupFailureCodes.Cancelled, result.FailureCode);
        Assert.IsInstanceOfType<OperationCanceledException>(result.PrimaryError);
        Assert.AreEqual(0, result.RecoveryErrors.Count);
        Assert.IsFalse(Directory.Exists(paths.GetDefaultInstallDirectory(product, InstallScope.CurrentUser)));
        Assert.IsFalse(File.Exists(paths.GetStateManifestPath(product.ProductId, InstallScope.CurrentUser)));
        AssertRecoveryRootHasNoOperations(paths, product.ProductId);
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldRestoreUpdateTreeAndStateByteForByteWhenCancelledAfterDeployment()
    {
        using TempDirectoryScope temp = new();
        string productDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "product")).FullName;
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, "demo-app", "DemoApp.exe", allowOverwrite: true);
        string v1Publish = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoApp.exe", "1.0.0", "v1.txt");
        string v1PackageDirectory = await SetupTestDataFactory.CreatePackageAsync(v1Publish, manifestPath, Path.Combine(temp.DirectoryPath, "packages", "v1"), "1.0.0");
        string v2Publish = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoApp.exe", "2.0.0", "v2.txt");
        string v2PackageDirectory = await SetupTestDataFactory.CreatePackageAsync(v2Publish, manifestPath, Path.Combine(temp.DirectoryPath, "packages", "v2"), "2.0.0");
        JsonManifestSerializer serializer = new();
        ProductManifest product = serializer.Load<ProductManifest>(manifestPath);
        TestSystemPaths paths = new(temp.DirectoryPath);
        SetupEngine engine = new(CreateServices(paths));
        PackageManifest v1Package = serializer.Load<PackageManifest>(Path.Combine(v1PackageDirectory, SetupRuntimeDefaults.PackageManifestFileName));
        SetupOperationResult installed = await engine.ExecuteAsync(CreateInstallOptions(paths, product, manifestPath, v1PackageDirectory, v1Package));
        Assert.IsTrue(installed.Succeeded, installed.Message);
        string installDirectory = installed.InstalledState!.InstallDirectory;
        string statePath = installed.InstalledState.StateManifestPath;
        string dataPath = installed.InstalledState.DataDirectories["userData"];
        File.WriteAllText(Path.Combine(dataPath, "user-data.txt"), "preserve-me");
        IReadOnlyDictionary<string, byte[]> originalInstall = CaptureTree(installDirectory);
        IReadOnlyDictionary<string, byte[]> originalData = CaptureTree(dataPath);
        byte[] originalState = File.ReadAllBytes(statePath);
        PackageManifest v2Package = serializer.Load<PackageManifest>(Path.Combine(v2PackageDirectory, SetupRuntimeDefaults.PackageManifestFileName));
        using CancellationTokenSource cancellation = new();

        SetupOperationResult cancelled = await engine.ExecuteAsync(
            CreateInstallOptions(paths, product, manifestPath, v2PackageDirectory, v2Package),
            new InlineProgress(step =>
            {
                if (step.Message == "Deploy maintenance runtime")
                {
                    cancellation.Cancel();
                }
            }),
            cancellation.Token);

        Assert.AreEqual(SetupOperationStatus.Cancelled, cancelled.Status);
        Assert.AreEqual(0, cancelled.RecoveryErrors.Count);
        AssertTreesEqual(originalInstall, CaptureTree(installDirectory));
        AssertTreesEqual(originalData, CaptureTree(dataPath));
        CollectionAssert.AreEqual(originalState, File.ReadAllBytes(statePath));
        AssertRecoveryRootHasNoOperations(paths, product.ProductId);
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldRestoreUninstallTreeStateAndDataByteForByteWhenCancelled()
    {
        using TempDirectoryScope temp = new();
        string productDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "product")).FullName;
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, "demo-app", "DemoApp.exe", allowOverwrite: true);
        string publishDirectory = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoApp.exe", "1.0.0", "v1.txt");
        string packageDirectory = await SetupTestDataFactory.CreatePackageAsync(publishDirectory, manifestPath, Path.Combine(temp.DirectoryPath, "packages"), "1.0.0");
        JsonManifestSerializer serializer = new();
        ProductManifest product = serializer.Load<ProductManifest>(manifestPath);
        TestSystemPaths paths = new(temp.DirectoryPath);
        SetupEngine engine = new(CreateServices(paths));
        PackageManifest package = serializer.Load<PackageManifest>(Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName));
        SetupOperationResult installed = await engine.ExecuteAsync(CreateInstallOptions(paths, product, manifestPath, packageDirectory, package));
        Assert.IsTrue(installed.Succeeded, installed.Message);
        string installDirectory = installed.InstalledState!.InstallDirectory;
        string statePath = installed.InstalledState.StateManifestPath;
        string dataPath = installed.InstalledState.DataDirectories["userData"];
        File.WriteAllText(Path.Combine(dataPath, "user-data.txt"), "preserve-me");
        IReadOnlyDictionary<string, byte[]> originalInstall = CaptureTree(installDirectory);
        IReadOnlyDictionary<string, byte[]> originalData = CaptureTree(dataPath);
        byte[] originalState = File.ReadAllBytes(statePath);
        using CancellationTokenSource cancellation = new();

        SetupOperationResult cancelled = await engine.ExecuteAsync(
            new RuntimeOptions
            {
                Mode = SetupMode.Uninstall,
                Scope = InstallScope.CurrentUser,
                ProductManifestPath = manifestPath,
                PurgeData = true
            },
            new InlineProgress(step =>
            {
                if (step.Message == "Remove product data")
                {
                    cancellation.Cancel();
                }
            }),
            cancellation.Token);

        Assert.AreEqual(SetupOperationStatus.Cancelled, cancelled.Status);
        Assert.AreEqual(0, cancelled.RecoveryErrors.Count);
        AssertTreesEqual(originalInstall, CaptureTree(installDirectory));
        AssertTreesEqual(originalData, CaptureTree(dataPath));
        CollectionAssert.AreEqual(originalState, File.ReadAllBytes(statePath));
        AssertRecoveryRootHasNoOperations(paths, product.ProductId);
    }

    private static SetupServices CreateServices(TestSystemPaths paths)
    {
        return TestSetupServicesFactory.Create(
            paths,
            new FakeRegistryService(),
            new FakeShortcutService(),
            new FakeProcessService(),
            new FakeDownloadService());
    }

    private static RuntimeOptions CreateInstallOptions(
        TestSystemPaths paths,
        ProductManifest product,
        string manifestPath,
        string packageDirectory,
        PackageManifest package)
    {
        return new RuntimeOptions
        {
            Mode = SetupMode.Install,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath,
            PackageManifestPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName),
            PackagePath = Path.Combine(packageDirectory, package.ArchiveFileName),
            InstallDirectory = paths.GetDefaultInstallDirectory(product, InstallScope.CurrentUser)
        };
    }

    private static IReadOnlyDictionary<string, byte[]> CaptureTree(string root)
    {
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(
                path => Path.GetRelativePath(root, path),
                File.ReadAllBytes,
                StringComparer.OrdinalIgnoreCase);
    }

    private static void AssertTreesEqual(IReadOnlyDictionary<string, byte[]> expected, IReadOnlyDictionary<string, byte[]> actual)
    {
        CollectionAssert.AreEquivalent(expected.Keys.ToArray(), actual.Keys.ToArray());
        foreach ((string path, byte[] expectedBytes) in expected)
        {
            CollectionAssert.AreEqual(expectedBytes, actual[path], path);
        }
    }

    private static void AssertRecoveryRootHasNoOperations(TestSystemPaths paths, string productId)
    {
        string root = paths.GetRecoveryRoot(productId, InstallScope.CurrentUser);
        if (Directory.Exists(root))
        {
            Assert.AreEqual(0, Directory.EnumerateDirectories(root).Count());
        }
    }

    private sealed class InlineProgress(Action<SetupProgress> report) : IProgress<SetupProgress>
    {
        public void Report(SetupProgress value) => report(value);
    }
}
