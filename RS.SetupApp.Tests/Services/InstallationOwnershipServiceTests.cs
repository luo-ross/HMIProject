using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class InstallationOwnershipServiceTests
{
    [TestMethod]
    public void Write_ShouldPreserveOriginalMarker_WhenOwnershipMatches()
    {
        using TempDirectoryScope temp = new();
        InstallationOwnershipService service = new(new PhysicalFileSystem(), new JsonManifestSerializer());
        Guid installationId = Guid.NewGuid();
        DateTimeOffset originalCreatedAt = new(2026, 7, 10, 1, 2, 3, TimeSpan.Zero);
        service.Write(temp.DirectoryPath, new InstallationOwnershipMarker
        {
            ProductId = "demo-app",
            InstallationId = installationId,
            InstallScope = InstallScope.CurrentUser,
            CreatedAtUtc = originalCreatedAt
        });

        service.Write(temp.DirectoryPath, new InstallationOwnershipMarker
        {
            ProductId = "demo-app",
            InstallationId = installationId,
            InstallScope = InstallScope.CurrentUser,
            CreatedAtUtc = originalCreatedAt.AddDays(1)
        });

        InstallationOwnershipMarker? persisted = service.Load(temp.DirectoryPath);
        Assert.IsNotNull(persisted);
        Assert.AreEqual(originalCreatedAt, persisted.CreatedAtUtc);
    }

    [TestMethod]
    public void Delete_ShouldRemovePersistedMarker()
    {
        using TempDirectoryScope temp = new();
        InstallationOwnershipService service = new(new PhysicalFileSystem(), new JsonManifestSerializer());
        service.Write(temp.DirectoryPath, new InstallationOwnershipMarker
        {
            ProductId = "demo-app",
            InstallationId = Guid.NewGuid(),
            InstallScope = InstallScope.CurrentUser
        });

        service.Delete(temp.DirectoryPath);

        Assert.IsNull(service.Load(temp.DirectoryPath));
    }

    [TestMethod]
    public void Load_ShouldReturnNull_WhenMarkerDoesNotExist()
    {
        using TempDirectoryScope temp = new();
        InstallationOwnershipService service = new(new PhysicalFileSystem(), new JsonManifestSerializer());

        InstallationOwnershipMarker? marker = service.Load(temp.DirectoryPath);

        Assert.IsNull(marker);
    }

    [TestMethod]
    public void Load_ShouldReturnPersistedMarker()
    {
        using TempDirectoryScope temp = new();
        JsonManifestSerializer serializer = new();
        InstallationOwnershipMarker expected = new()
        {
            ProductId = "demo-app",
            InstallationId = Guid.Parse("9da2b782-b956-4f32-9860-56cd267ca2cb"),
            InstallScope = InstallScope.AllUsers
        };
        serializer.Save(Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.OwnershipMarkerFileName), expected);
        InstallationOwnershipService service = new(new PhysicalFileSystem(), serializer);

        InstallationOwnershipMarker? actual = service.Load(temp.DirectoryPath);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.ProductId, actual.ProductId);
        Assert.AreEqual(expected.InstallationId, actual.InstallationId);
        Assert.AreEqual(expected.InstallScope, actual.InstallScope);
    }

    [TestMethod]
    public void Write_ShouldPersistOwnershipMarker()
    {
        using TempDirectoryScope temp = new();
        JsonManifestSerializer serializer = new();
        InstallationOwnershipService service = new(new PhysicalFileSystem(), serializer);
        InstallationOwnershipMarker marker = new()
        {
            SchemaVersion = 1,
            ProductId = "demo-app",
            InstallationId = Guid.Parse("18122090-5bb0-4d57-bdb7-d076c67ae784"),
            InstallScope = InstallScope.CurrentUser,
            CreatedAtUtc = new DateTimeOffset(2026, 7, 10, 1, 2, 3, TimeSpan.Zero)
        };

        service.Write(temp.DirectoryPath, marker);

        string markerPath = Path.Combine(temp.DirectoryPath, SetupRuntimeDefaults.OwnershipMarkerFileName);
        InstallationOwnershipMarker persisted = serializer.Load<InstallationOwnershipMarker>(markerPath);
        Assert.AreEqual(marker.SchemaVersion, persisted.SchemaVersion);
        Assert.AreEqual(marker.ProductId, persisted.ProductId);
        Assert.AreEqual(marker.InstallationId, persisted.InstallationId);
        Assert.AreEqual(marker.InstallScope, persisted.InstallScope);
        Assert.AreEqual(marker.CreatedAtUtc, persisted.CreatedAtUtc);
    }
}
