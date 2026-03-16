using System.Text;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class ProductManifestLoaderTests
{
    [TestMethod]
    public void Load_ShouldReturnSchemaErrorsForUnexpectedProperties()
    {
        using TempDirectoryScope temp = new();
        SetupTestDataFactory.WriteProductSchema(temp.DirectoryPath);
        File.WriteAllText(Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.ProductManifestFileName), """
        {
          "productId": "demo-app",
          "displayName": "Demo App",
          "publisher": "Contoso",
          "mainExecutable": "Demo.exe",
          "unexpected": true
        }
        """, Encoding.UTF8);

        ProductManifestLoadResult result = ProductManifestLoader.Load(
            Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.ProductManifestFileName),
            new JsonManifestSerializer());

        Assert.IsTrue(result.Errors.Any(error => error.Contains("additional properties are not allowed", StringComparison.OrdinalIgnoreCase)));
    }
}
