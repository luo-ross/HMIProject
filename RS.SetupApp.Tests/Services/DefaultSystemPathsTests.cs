using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class DefaultSystemPathsTests
{
    [TestMethod]
    public void GetDefaultInstallDirectory_ShouldUseScopeSpecificRoots()
    {
        ProductManifest manifest = new()
        {
            ProductId = "demo-app",
            DisplayName = "Demo App",
            Publisher = "Acme",
            MainExecutable = "Demo.exe"
        };

        DefaultSystemPaths paths = new();

        string userInstallDirectory = paths.GetDefaultInstallDirectory(manifest, InstallScope.CurrentUser);
        string machineInstallDirectory = paths.GetDefaultInstallDirectory(manifest, InstallScope.AllUsers);

        StringAssert.Contains(userInstallDirectory, "Programs");
        StringAssert.Contains(machineInstallDirectory, "Acme");
        Assert.AreNotEqual(userInstallDirectory, machineInstallDirectory);
    }
}
