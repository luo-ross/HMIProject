using System.Runtime.Versioning;
using Microsoft.Win32;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Live;

[TestClass]
[SupportedOSPlatform("windows")]
public sealed class WindowsIntegrationLiveTests
{
    [TestMethod]
    public async Task ExecuteAsync_ShouldRegisterAndRemoveCurrentUserRegistryEntries_WhenLiveTestsAreEnabled()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RS_SETUPAPP_LIVE_TESTS"), "1", StringComparison.Ordinal))
        {
            return;
        }

        using TempDirectoryScope temp = new();
        string productDirectory = Path.Combine(temp.DirectoryPath, "product");
        Directory.CreateDirectory(productDirectory);
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, "demo-live-app", "DemoLive.exe");
        JsonManifestSerializer serializer = new();
        ProductManifest product = serializer.Load<ProductManifest>(manifestPath);
        product.FileAssociations.Add(new FileAssociationManifest
        {
            Extension = ".livecfg",
            ProgId = "Contoso.DemoLive.Config",
            FriendlyName = "Demo Live Config"
        });
        serializer.Save(manifestPath, product);

        string publishDirectory = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoLive.exe", "1.0.0");
        string packageDirectory = await SetupTestDataFactory.CreatePackageAsync(
            publishDirectory,
            manifestPath,
            Path.Combine(temp.DirectoryPath, "packages")).ConfigureAwait(false);
        PackageManifest package = serializer.Load<PackageManifest>(Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName));

        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem fileSystem = new();
        InstallationOwnershipService ownershipService = new(fileSystem, serializer);
        SetupPathSafetyPolicy pathSafetyPolicy = new(fileSystem, ownershipService);
        SetupServices services = new()
        {
            FileSystem = fileSystem,
            Serializer = serializer,
            Registry = new WindowsRegistryService(),
            Shortcuts = new ShellShortcutService(paths),
            Processes = new ProcessService(),
            Downloads = new HttpDownloadService(),
            Hasher = new DefaultFileHasher(),
            Paths = paths,
            PathSafetyPolicy = pathSafetyPolicy,
            OwnershipService = ownershipService,
            InstalledStateValidator = new InstalledStateValidator(
                fileSystem,
                paths,
                ownershipService,
                pathSafetyPolicy),
            LegacyInstallationClaimService = new LegacyInstallationClaimService(
                fileSystem,
                paths,
                serializer,
                ownershipService,
                pathSafetyPolicy),
            LoggerFactory = path => new FileSetupLogger(path)
        };

        SetupEngine engine = new(services);
        SetupOperationResult installResult = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Install,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath,
            PackageManifestPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName),
            PackagePath = Path.Combine(packageDirectory, package.ArchiveFileName),
            InstallDirectory = paths.GetDefaultInstallDirectory(product, InstallScope.CurrentUser)
        }).ConfigureAwait(false);

        Assert.IsTrue(installResult.Succeeded);
        using RegistryKey? uninstallKey = Registry.CurrentUser.OpenSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{product.ProductId}");
        Assert.IsNotNull(uninstallKey);
        using RegistryKey? extensionKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.livecfg");
        Assert.IsNotNull(extensionKey);

        SetupOperationResult uninstallResult = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Uninstall,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = manifestPath
        }).ConfigureAwait(false);

        Assert.IsTrue(uninstallResult.Succeeded);
        Assert.IsNull(Registry.CurrentUser.OpenSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{product.ProductId}"));
        Assert.IsNull(Registry.CurrentUser.OpenSubKey(@"Software\Classes\.livecfg"));
    }
}
