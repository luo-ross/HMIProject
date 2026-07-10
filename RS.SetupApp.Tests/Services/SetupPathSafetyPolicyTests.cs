using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class SetupPathSafetyPolicyTests
{
    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectUnreadableOwnershipMarker()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "existing")).FullName;
        File.WriteAllText(
            Path.Combine(installDirectory, SetupRuntimeDefaults.OwnershipMarkerFileName),
            "not-json");
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(allowOverwrite: true),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.OwnershipMismatch, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectExistingFile()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Path.Combine(temp.DirectoryPath, "not-a-directory");
        File.WriteAllText(installDirectory, "content");
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.InvalidPath, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectOwnedTarget_WhenOverwriteIsDisabled()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "existing")).FullName;
        Guid installationId = Guid.NewGuid();
        WriteMarker(installDirectory, "demo-app", installationId, InstallScope.CurrentUser);
        InstalledStateManifest installedState = new()
        {
            ProductId = "demo-app",
            InstallationId = installationId,
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = installDirectory
        };
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(allowOverwrite: false),
            InstallScope.CurrentUser,
            installedState);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.OverwriteDisabled, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldAllowMatchingMarkerAndState()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "existing")).FullName;
        Guid installationId = Guid.NewGuid();
        WriteMarker(installDirectory, "demo-app", installationId, InstallScope.CurrentUser);
        InstalledStateManifest installedState = new()
        {
            ProductId = "demo-app",
            InstallationId = installationId,
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = installDirectory
        };
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(allowOverwrite: true),
            InstallScope.CurrentUser,
            installedState);

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.None, result.FailureCode);
        Assert.AreEqual(Path.GetFullPath(installDirectory), result.NormalizedPath);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectMismatchedInstallDirectory()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "existing")).FullName;
        Guid installationId = Guid.NewGuid();
        WriteMarker(installDirectory, "demo-app", installationId, InstallScope.CurrentUser);
        InstalledStateManifest installedState = new()
        {
            ProductId = "demo-app",
            InstallationId = installationId,
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = Path.Combine(temp.DirectoryPath, "different")
        };
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(allowOverwrite: true),
            InstallScope.CurrentUser,
            installedState);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.OwnershipMismatch, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectMismatchedOwnershipScope()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "existing")).FullName;
        Guid installationId = Guid.NewGuid();
        WriteMarker(installDirectory, "demo-app", installationId, InstallScope.AllUsers);
        InstalledStateManifest installedState = new()
        {
            ProductId = "demo-app",
            InstallationId = installationId,
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = installDirectory
        };
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(allowOverwrite: true),
            InstallScope.CurrentUser,
            installedState);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.ScopeMismatch, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectMismatchedInstallationId()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "existing")).FullName;
        WriteMarker(installDirectory, "demo-app", Guid.NewGuid(), InstallScope.CurrentUser);
        InstalledStateManifest installedState = new()
        {
            ProductId = "demo-app",
            InstallationId = Guid.NewGuid(),
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = installDirectory
        };
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(allowOverwrite: true),
            InstallScope.CurrentUser,
            installedState);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.OwnershipMismatch, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectMismatchedProduct()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "existing")).FullName;
        Guid installationId = Guid.NewGuid();
        WriteMarker(installDirectory, "another-product", installationId, InstallScope.CurrentUser);
        InstalledStateManifest installedState = new()
        {
            ProductId = "demo-app",
            InstallationId = installationId,
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = installDirectory
        };
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(allowOverwrite: true),
            InstallScope.CurrentUser,
            installedState);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.OwnershipMismatch, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectNonEmptyUnownedDirectory()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "existing")).FullName;
        string sentinelPath = Path.Combine(installDirectory, "sentinel.txt");
        File.WriteAllText(sentinelPath, "do not overwrite");
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(allowOverwrite: true),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.NonEmptyUnownedDirectory, result.FailureCode);
        Assert.AreEqual("do not overwrite", File.ReadAllText(sentinelPath));
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectReparsePoint()
    {
        using TempDirectoryScope temp = new();
        string realDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "real")).FullName;
        string installDirectory = Path.Combine(temp.DirectoryPath, "linked-install");
        Directory.CreateSymbolicLink(installDirectory, realDirectory);
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.ReparsePointNotTrusted, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectNestedReparsePoint()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "install")).FullName;
        string realDirectory = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "real")).FullName;
        Directory.CreateSymbolicLink(Path.Combine(installDirectory, "linked-child"), realDirectory);
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.ReparsePointNotTrusted, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldAllowChildBelowLocalAppData()
    {
        string installDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Contoso",
            $"DemoApp-{Guid.NewGuid():N}");
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldAllowAllUsersChildBelowProgramFiles()
    {
        string installDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Contoso",
            $"DemoApp-{Guid.NewGuid():N}");
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(),
            InstallScope.AllUsers,
            installedState: null);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectAllUsersTargetOutsideProgramFiles()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Path.Combine(temp.DirectoryPath, "DemoApp");
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(),
            InstallScope.AllUsers,
            installedState: null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.ScopeMismatch, result.FailureCode);
    }

    [DataTestMethod]
    [DynamicData(nameof(GetSpecialFolderRoots), DynamicDataSourceType.Method)]
    public void ValidateInstallTarget_ShouldRejectSpecialFolderRoot(string specialFolderRoot)
    {
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            specialFolderRoot,
            CreateProduct(),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.SpecialFolderRoot, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectWindowsTree()
    {
        string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        Assert.IsFalse(string.IsNullOrWhiteSpace(windowsDirectory));
        string systemDirectory = Path.Combine(windowsDirectory, "System32");
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            systemDirectory,
            CreateProduct(),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InstallTargetFailureCode.WindowsDirectory, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectDriveRoot()
    {
        using TempDirectoryScope temp = new();
        string driveRoot = Path.GetPathRoot(temp.DirectoryPath)
            ?? throw new InvalidOperationException("Test directory has no drive root.");
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            driveRoot,
            CreateProduct(),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(Path.GetFullPath(driveRoot), result.NormalizedPath);
        Assert.AreEqual(InstallTargetFailureCode.DriveRoot, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldRejectInvalidPath()
    {
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            "invalid\0path",
            CreateProduct(),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsFalse(result.IsValid);
        Assert.IsNull(result.NormalizedPath);
        Assert.AreEqual(InstallTargetFailureCode.InvalidPath, result.FailureCode);
    }

    [TestMethod]
    public void ValidateInstallTarget_ShouldAllowNewEmptyProductDirectory()
    {
        using TempDirectoryScope temp = new();
        string installDirectory = Path.Combine(temp.DirectoryPath, "Contoso", "DemoApp");
        Directory.CreateDirectory(installDirectory);
        SetupPathSafetyPolicy policy = CreatePolicy();

        InstallTargetValidationResult result = policy.ValidateInstallTarget(
            installDirectory,
            CreateProduct(),
            InstallScope.CurrentUser,
            installedState: null);

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(Path.GetFullPath(installDirectory), result.NormalizedPath);
        Assert.AreEqual(InstallTargetFailureCode.None, result.FailureCode);
    }

    private static ProductManifest CreateProduct(bool allowOverwrite = false)
    {
        return new ProductManifest
        {
            ProductId = "demo-app",
            InstallDefaults = new InstallDefaultsManifest
            {
                AllowOverwrite = allowOverwrite
            }
        };
    }

    public static IEnumerable<object[]> GetSpecialFolderRoots()
    {
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] candidates =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            userProfile,
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            string.IsNullOrWhiteSpace(userProfile) ? string.Empty : Path.Combine(userProfile, "Downloads"),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)
        ];

        return candidates
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static path => new object[] { path });
    }

    private static void WriteMarker(
        string installDirectory,
        string productId,
        Guid installationId,
        InstallScope scope)
    {
        new InstallationOwnershipService(new PhysicalFileSystem(), new JsonManifestSerializer()).Write(
            installDirectory,
            new InstallationOwnershipMarker
            {
                ProductId = productId,
                InstallationId = installationId,
                InstallScope = scope
            });
    }

    private static SetupPathSafetyPolicy CreatePolicy()
    {
        PhysicalFileSystem fileSystem = new();
        return new SetupPathSafetyPolicy(
            fileSystem,
            new InstallationOwnershipService(fileSystem, new JsonManifestSerializer()));
    }
}
