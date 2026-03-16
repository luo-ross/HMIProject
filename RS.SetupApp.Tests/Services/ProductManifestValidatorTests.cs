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
}
