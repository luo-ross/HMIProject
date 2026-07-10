using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class DefaultSystemPathsTests
{
    [TestMethod]
    public void GetRecoveryDirectory_ShouldUseOperationDirectoryBelowProductRoot()
    {
        DefaultSystemPaths paths = new();
        Guid operationId = Guid.Parse("92f08a6c-23c1-479c-9fce-cb55735af4db");

        string recoveryDirectory = paths.GetRecoveryDirectory(
            "demo-app",
            operationId,
            InstallScope.CurrentUser);

        Assert.AreEqual(
            Path.Combine(paths.GetRecoveryRoot("demo-app", InstallScope.CurrentUser), operationId.ToString("N")),
            recoveryDirectory);
    }

    [TestMethod]
    public void GetRecoveryRoot_ShouldUseScopeSpecificApplicationDataRoots()
    {
        DefaultSystemPaths paths = new();

        string userRecoveryRoot = paths.GetRecoveryRoot("demo-app", InstallScope.CurrentUser);
        string machineRecoveryRoot = paths.GetRecoveryRoot("demo-app", InstallScope.AllUsers);

        StringAssert.StartsWith(
            userRecoveryRoot,
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            StringComparison.OrdinalIgnoreCase);
        StringAssert.StartsWith(
            machineRecoveryRoot,
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            StringComparison.OrdinalIgnoreCase);
        StringAssert.EndsWith(userRecoveryRoot, Path.Combine("Recovery", "demo-app"));
        StringAssert.EndsWith(machineRecoveryRoot, Path.Combine("Recovery", "demo-app"));
    }

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
