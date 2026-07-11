using RS.SetupApp.Builder;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;
using System.Security.Cryptography;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class UpdateFeedPublisherTests
{
    [TestMethod]
    public void Publish_ShouldCreateLatestJsonWithResolvedUrls()
    {
        using TempDirectoryScope temp = new();
        JsonManifestSerializer serializer = new();
        PackageManifest manifest = new()
        {
            ProductId = "demo-app",
            Version = "1.2.3",
            MainExecutable = "Demo.exe",
            ArchiveFileName = "demo-app-1.2.3.zip",
            ArchiveSha256 = "abc123"
        };

        serializer.Save(Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.PackageManifestFileName), manifest);

        string signingKey = WriteSigningKey(temp.DirectoryPath);
        UpdateFeedPublisher publisher = new(serializer);
        string outputPath = publisher.Publish(new BuilderOptions
        {
            Command = BuilderCommand.PublishUpdateFeed,
            PackageDirectory = temp.DirectoryPath,
            BaseUrl = "https://example.com/downloads/",
            SigningKeyPath = signingKey
        });

        UpdateFeedManifest updateFeed = serializer.Load<UpdateFeedManifest>(outputPath);
        Assert.AreEqual("https://example.com/downloads/demo-app-1.2.3.zip", updateFeed.PackageUrl);
        Assert.AreEqual("https://example.com/downloads/package.manifest.json", updateFeed.PackageManifestUrl);
        Assert.AreEqual("1.2.3", updateFeed.Version);
        Assert.IsTrue(File.Exists(Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.PackageManifestSignatureFileName)));
        Assert.IsTrue(File.Exists(Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.UpdateManifestSignatureFileName)));
        Assert.IsTrue(File.Exists(Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.TrustedPublicKeyFileName)));
        RsaPssUpdateSignatureVerifier verifier = new();
        Assert.IsTrue(verifier.Verify(
            outputPath,
            Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.UpdateManifestSignatureFileName),
            Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.TrustedPublicKeyFileName)));
    }

    [TestMethod]
    public void Publish_ShouldRequireSigningKeyAndRejectHttpBaseUrl()
    {
        using TempDirectoryScope temp = new();
        JsonManifestSerializer serializer = new();
        serializer.Save(Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.PackageManifestFileName), new PackageManifest
        {
            ProductId = "demo-app",
            Version = "1.2.3",
            MainExecutable = "Demo.exe",
            ArchiveFileName = "demo-app-1.2.3.zip",
            ArchiveSha256 = "abc123"
        });

        UpdateFeedPublisher publisher = new(serializer);
        Assert.ThrowsException<ArgumentException>(() => publisher.Publish(new BuilderOptions
        {
            Command = BuilderCommand.PublishUpdateFeed,
            PackageDirectory = temp.DirectoryPath
        }));
        Assert.ThrowsException<InvalidOperationException>(() => publisher.Publish(new BuilderOptions
        {
            Command = BuilderCommand.PublishUpdateFeed,
            PackageDirectory = temp.DirectoryPath,
            BaseUrl = "http://example.test/downloads/",
            SigningKeyPath = WriteSigningKey(temp.DirectoryPath)
        }));
    }

    private static string WriteSigningKey(string directoryPath)
    {
        string signingKey = Path.Combine(directoryPath, "signing.private.pem");
        using RSA rsa = RSA.Create(2048);
        File.WriteAllText(signingKey, rsa.ExportRSAPrivateKeyPem());
        return signingKey;
    }
}
