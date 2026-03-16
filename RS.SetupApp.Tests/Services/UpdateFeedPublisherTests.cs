using RS.SetupApp.Builder;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;

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

        UpdateFeedPublisher publisher = new(serializer);
        string outputPath = publisher.Publish(new BuilderOptions
        {
            Command = BuilderCommand.PublishUpdateFeed,
            PackageDirectory = temp.DirectoryPath,
            BaseUrl = "https://example.com/downloads/"
        });

        UpdateFeedManifest updateFeed = serializer.Load<UpdateFeedManifest>(outputPath);
        Assert.AreEqual("https://example.com/downloads/demo-app-1.2.3.zip", updateFeed.PackageUrl);
        Assert.AreEqual("https://example.com/downloads/package.manifest.json", updateFeed.PackageManifestUrl);
        Assert.AreEqual("1.2.3", updateFeed.Version);
    }
}
