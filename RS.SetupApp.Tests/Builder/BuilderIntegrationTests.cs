using RS.SetupApp.Builder;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;
using System.Security.Cryptography;

namespace RS.SetupApp.Tests.Builder;

[TestClass]
public sealed class BuilderIntegrationTests
{
    [TestMethod]
    public async Task BuildInstaller_ShouldProduceRuntimeAndPayloadArtifacts()
    {
        using TempDirectoryScope temp = new();
        string productDirectory = Path.Combine(temp.DirectoryPath, "product");
        Directory.CreateDirectory(productDirectory);
        SetupTestDataFactory.WriteProductSchema(productDirectory);
        string manifestPath = SetupTestDataFactory.WriteProductManifest(productDirectory, "demo-app", "DemoApp.exe");

        string publishDirectory = SetupTestDataFactory.CreatePublishDirectory(temp.DirectoryPath, "DemoApp.exe", "1.0.0", "data\\payload.txt");
        string packageDirectory = await SetupTestDataFactory.CreatePackageAsync(
            publishDirectory,
            manifestPath,
            Path.Combine(temp.DirectoryPath, "packages"),
            packageVersion: "1.0.0").ConfigureAwait(false);

        JsonManifestSerializer serializer = new();
        ProductManifest product = serializer.Load<ProductManifest>(manifestPath);
        product.Update = new UpdateSettingsManifest
        {
            AllowOnlineUpdate = true,
            RequireHttps = true,
            RequireSignature = true,
            TrustedPublicKeyPath = "keys/update.public.pem",
            ManifestUrl = "https://example.test/downloads/latest.json"
        };
        serializer.Save(manifestPath, product);
        string signingKeyPath = Path.Combine(temp.DirectoryPath, "release.private.pem");
        using (RSA rsa = RSA.Create(2048))
        {
            File.WriteAllText(signingKeyPath, rsa.ExportRSAPrivateKeyPem());
        }
        UpdateFeedPublisher updateFeedPublisher = new(serializer);
        updateFeedPublisher.Publish(new BuilderOptions
        {
            Command = BuilderCommand.PublishUpdateFeed,
            PackageDirectory = packageDirectory,
            ProductManifestPath = manifestPath,
            BaseUrl = "https://example.test/downloads/",
            SigningKeyPath = signingKeyPath
        });

        string runtimeProjectDirectory = Path.Combine(temp.DirectoryPath, "runtime-project");
        Directory.CreateDirectory(runtimeProjectDirectory);
        File.WriteAllText(Path.Combine(runtimeProjectDirectory, "Runtime.csproj"), """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>net9.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>
        </Project>
        """);
        File.WriteAllText(Path.Combine(runtimeProjectDirectory, "Program.cs"), "Console.WriteLine(\"setup runtime\");");

        InstallerBundleBuilder builder = new(serializer, new DotnetPublishRunner());
        string outputDirectory = await builder.BuildAsync(new BuilderOptions
        {
            Command = BuilderCommand.BuildInstaller,
            ProductManifestPath = manifestPath,
            PackageDirectory = packageDirectory,
            OutputDirectory = Path.Combine(temp.DirectoryPath, "installer"),
            RuntimeProjectPath = Path.Combine(runtimeProjectDirectory, "Runtime.csproj"),
            Configuration = "Release",
            Runtime = "win-x64"
        }, CancellationToken.None).ConfigureAwait(false);

        Assert.IsTrue(Directory.Exists(outputDirectory));
        Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, SetupRuntimeDefaults.ProductManifestFileName)));
        Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, SetupRuntimeDefaults.PackageManifestFileName)));
        Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, SetupRuntimeDefaults.UpdateManifestFileName)));
        Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, SetupRuntimeDefaults.PackageManifestSignatureFileName)));
        Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, SetupRuntimeDefaults.UpdateManifestSignatureFileName)));
        Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, "keys", "update.public.pem")));
        Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, "checksums.txt")));
        Assert.IsTrue(Directory.GetFiles(outputDirectory, "*.exe", SearchOption.TopDirectoryOnly).Length > 0);
    }
}
