using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class ProductManifestLocatorTests
{
    [TestMethod]
    public void Resolve_ShouldPreferPayloadManifest_WhenPayloadContainsProductManifest()
    {
        using TempDirectoryScope temp = new();
        string previousDirectory = Directory.GetCurrentDirectory();
        TestSystemPaths paths = new(temp.DirectoryPath);
        string payloadManifestPath = Path.Combine(paths.GetPayloadDirectory(), SetupRuntimeDefaults.ProductManifestFileName);
        File.WriteAllText(payloadManifestPath, "{}");

        Directory.SetCurrentDirectory(temp.DirectoryPath);

        try
        {
            ProductManifestLocationResult result = ProductManifestLocator.Resolve(new RuntimeOptions(), paths, new PhysicalFileSystem());

            Assert.IsTrue(result.Exists);
            Assert.AreEqual(Path.GetFullPath(payloadManifestPath), result.ResolvedPath);
        }
        finally
        {
            Directory.SetCurrentDirectory(previousDirectory);
        }
    }

    [TestMethod]
    public void Resolve_ShouldFallbackToTemplateManifest_WhenRunningFromProjectOutput()
    {
        using TempDirectoryScope temp = new();
        string previousDirectory = Directory.GetCurrentDirectory();
        string appBaseDirectory = Path.Combine(temp.DirectoryPath, "RS.SetupApp", "bin", "Debug", "net9.0-windows");
        Directory.CreateDirectory(appBaseDirectory);

        string templateDirectory = Path.Combine(temp.DirectoryPath, "Templates", "RS.SetupApp.Template");
        Directory.CreateDirectory(templateDirectory);
        string templateManifestPath = Path.Combine(templateDirectory, SetupRuntimeDefaults.ProductManifestFileName);
        File.WriteAllText(templateManifestPath, "{}");

        DefaultSystemPaths paths = new(appBaseDirectory);
        Directory.SetCurrentDirectory(temp.DirectoryPath);

        try
        {
            ProductManifestLocationResult result = ProductManifestLocator.Resolve(new RuntimeOptions(), paths, new PhysicalFileSystem());

            Assert.IsTrue(result.Exists);
            Assert.AreEqual(Path.GetFullPath(templateManifestPath), result.ResolvedPath);
        }
        finally
        {
            Directory.SetCurrentDirectory(previousDirectory);
        }
    }

    [TestMethod]
    public void Resolve_ShouldReportSearchedPaths_WhenManifestIsMissing()
    {
        using TempDirectoryScope temp = new();
        string previousDirectory = Directory.GetCurrentDirectory();
        DefaultSystemPaths paths = new(Path.Combine(temp.DirectoryPath, "runtime"));
        Directory.SetCurrentDirectory(temp.DirectoryPath);

        try
        {
            ProductManifestLocationResult result = ProductManifestLocator.Resolve(new RuntimeOptions(), paths, new PhysicalFileSystem());

            Assert.IsFalse(result.Exists);
            Assert.IsTrue(result.SearchedPaths.Count >= 2);
            Assert.IsTrue(result.SearchedPaths.All(static path => !string.IsNullOrWhiteSpace(path)));
        }
        finally
        {
            Directory.SetCurrentDirectory(previousDirectory);
        }
    }
}
