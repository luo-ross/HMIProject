using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class ProductManifestValidatorTests
{
    [TestMethod]
    public void Validate_ShouldReturnErrorsForMissingRequiredFields()
    {
        ProductManifest manifest = new();

        IReadOnlyList<string> errors = ProductManifestValidator.Validate(manifest);

        Assert.IsTrue(errors.Count >= 4);
        CollectionAssert.Contains(errors.ToList(), "productId is required.");
        CollectionAssert.Contains(errors.ToList(), "displayName is required.");
        CollectionAssert.Contains(errors.ToList(), "publisher is required.");
        CollectionAssert.Contains(errors.ToList(), "mainExecutable is required.");
    }

    [TestMethod]
    public void Validate_ShouldRejectDuplicateDataDirectoriesAndParentTraversal()
    {
        ProductManifest manifest = new()
        {
            ProductId = "demo-app",
            DisplayName = "Demo App",
            Publisher = "Contoso",
            MainExecutable = "Demo.exe",
            DataDirectories =
            [
                new DataDirectoryManifest
                {
                    Key = "data",
                    Scope = DataDirectoryScope.UserLocal,
                    RelativePath = "Contoso\\Demo"
                },
                new DataDirectoryManifest
                {
                    Key = "data",
                    Scope = DataDirectoryScope.UserLocal,
                    RelativePath = "..\\escape"
                }
            ]
        };

        IReadOnlyList<string> errors = ProductManifestValidator.Validate(manifest);

        CollectionAssert.Contains(errors.ToList(), "dataDirectories contains duplicate keys.");
        CollectionAssert.Contains(errors.ToList(), "dataDirectories 'data' relativePath must stay within the configured data root.");
    }

    [TestMethod]
    public void Validate_ShouldRejectOnlineUpdatePublicKeyEscapeAndInsecureUrl()
    {
        using Helpers.TempDirectoryScope temp = new();
        string productManifestPath = Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.ProductManifestFileName);
        ProductManifest manifest = new()
        {
            ProductId = "demo-app",
            DisplayName = "Demo App",
            Publisher = "Contoso",
            MainExecutable = "Demo.exe",
            Update = new UpdateSettingsManifest
            {
                AllowOnlineUpdate = true,
                ManifestUrl = "http://example.test/latest.json",
                RequireHttps = true,
                RequireSignature = true,
                TrustedPublicKeyPath = "../escape.pem"
            }
        };

        IReadOnlyList<string> errors = ProductManifestValidator.Validate(manifest, productManifestPath, File.Exists);

        CollectionAssert.Contains(errors.ToList(), "update.manifestUrl must use HTTPS when allowOnlineUpdate is true.");
        CollectionAssert.Contains(errors.ToList(), "update.trustedPublicKeyPath must be relative to the product manifest directory and cannot contain '..'.");
    }
}
