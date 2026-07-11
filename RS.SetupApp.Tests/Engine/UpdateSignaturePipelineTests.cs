using System.Security.Cryptography;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Engine;

[TestClass]
public sealed class UpdateSignaturePipelineTests
{
    [TestMethod]
    public async Task CheckForUpdatesAsync_ShouldRejectTamperedFeedBeforeDeserialization()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        JsonManifestSerializer serializer = new();
        string productManifestPath = CreateOnlineProduct(temp.DirectoryPath, serializer, out string privateKeyPath);
        string updateDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "updates")).FullName;
        string feedPath = Path.Combine(updateDirectory, SetupRuntimeDefaults.UpdateManifestFileName);
        serializer.Save(feedPath, new UpdateFeedManifest { ProductId = "demo-app", Channel = "stable", Version = "2.0.0" });
        new RsaPssManifestSigner().Sign(feedPath, privateKeyPath);
        File.WriteAllText(feedPath, "this is not json and was modified after signing");

        MappingDownloadService downloads = new();
        downloads.Add("https://updates.example.test/latest.json", feedPath);
        downloads.Add("https://updates.example.test/latest.json.sig", $"{feedPath}.sig");
        SetupEngine engine = new(TestSetupServicesFactory.Create(
            paths,
            new FakeRegistryService(),
            new FakeShortcutService(),
            new FakeProcessService(),
            downloads));

        InvalidOperationException exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => engine.CheckForUpdatesAsync(productManifestPath));

        StringAssert.Contains(exception.Message, "signature verification failed");
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldRejectTamperedPackageManifestBeforeDeserialization()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        JsonManifestSerializer serializer = new();
        string productManifestPath = CreateOnlineProduct(temp.DirectoryPath, serializer, out string privateKeyPath);
        string updateDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "updates")).FullName;
        string feedPath = Path.Combine(updateDirectory, SetupRuntimeDefaults.UpdateManifestFileName);
        serializer.Save(feedPath, new UpdateFeedManifest
        {
            ProductId = "demo-app",
            Channel = "stable",
            Version = "2.0.0",
            PackageUrl = "package.zip",
            PackageManifestUrl = SetupRuntimeDefaults.PackageManifestFileName
        });
        RsaPssManifestSigner signer = new();
        signer.Sign(feedPath, privateKeyPath);
        string packageManifestPath = Path.Combine(updateDirectory, SetupRuntimeDefaults.PackageManifestFileName);
        File.WriteAllText(packageManifestPath, "not json");
        File.WriteAllText($"{packageManifestPath}.sig", "invalid signature");

        SetupEngine engine = new(TestSetupServicesFactory.Create(
            paths,
            new FakeRegistryService(),
            new FakeShortcutService(),
            new FakeProcessService(),
            new FakeDownloadService()));

        SetupOperationResult result = await engine.ExecuteAsync(new RuntimeOptions
        {
            Mode = SetupMode.Install,
            ProductManifestPath = productManifestPath,
            UpdateManifestPath = feedPath,
            InstallDirectory = paths.GetDefaultInstallDirectory(serializer.Load<ProductManifest>(productManifestPath), InstallScope.CurrentUser)
        });

        Assert.IsFalse(result.Succeeded);
        StringAssert.Contains(result.Message, "package.manifest.json");
        StringAssert.Contains(result.Message, "signature verification failed");
    }

    private static string CreateOnlineProduct(string directoryPath, JsonManifestSerializer serializer, out string privateKeyPath)
    {
        SetupTestDataFactory.WriteProductSchema(directoryPath);
        string productManifestPath = SetupTestDataFactory.WriteProductManifest(directoryPath, "demo-app", "Demo.exe");
        ProductManifest product = serializer.Load<ProductManifest>(productManifestPath);
        product.Update = new UpdateSettingsManifest
        {
            AllowOnlineUpdate = true,
            RequireHttps = true,
            RequireSignature = true,
            TrustedPublicKeyPath = "keys/update.public.pem",
            ManifestUrl = "https://updates.example.test/latest.json"
        };
        serializer.Save(productManifestPath, product);

        privateKeyPath = Path.Combine(directoryPath, "release.private.pem");
        using RSA rsa = RSA.Create(2048);
        File.WriteAllText(privateKeyPath, rsa.ExportRSAPrivateKeyPem());
        string publicKeyPath = Path.Combine(directoryPath, product.Update.TrustedPublicKeyPath);
        Directory.CreateDirectory(Path.GetDirectoryName(publicKeyPath)!);
        File.WriteAllText(publicKeyPath, rsa.ExportRSAPublicKeyPem());
        return productManifestPath;
    }

    private sealed class MappingDownloadService : IDownloadService
    {
        private readonly Dictionary<string, string> _sources = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string sourceUri, string sourcePath) => _sources[sourceUri] = sourcePath;

        public Task DownloadAsync(Uri uri, string destinationPath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_sources.TryGetValue(uri.ToString(), out string? sourcePath))
            {
                throw new InvalidOperationException($"Unexpected update request '{uri}'.");
            }

            File.Copy(sourcePath, destinationPath, overwrite: true);
            return Task.CompletedTask;
        }
    }
}
