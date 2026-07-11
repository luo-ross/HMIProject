using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class PhysicalFileSystemAtomicWriteTests
{
    [TestMethod]
    public void EnumerateDirectories_ShouldIncludeNestedDirectories()
    {
        using TempDirectoryScope temp = new();
        string child = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "child")).FullName;
        string grandchild = Directory.CreateDirectory(Path.Combine(child, "grandchild")).FullName;
        PhysicalFileSystem fileSystem = new();

        string[] directories = fileSystem
            .EnumerateDirectories(temp.DirectoryPath, "*", SearchOption.AllDirectories)
            .ToArray();

        CollectionAssert.AreEquivalent(new[] { child, grandchild }, directories);
    }

    [TestMethod]
    public void WriteAllTextAtomic_ShouldReplaceExistingFile()
    {
        using TempDirectoryScope temp = new();
        string path = Path.Combine(temp.DirectoryPath, "state.json");
        File.WriteAllText(path, "old");
        PhysicalFileSystem fileSystem = new();

        fileSystem.WriteAllTextAtomic(path, "new");

        Assert.AreEqual("new", File.ReadAllText(path));
        CollectionAssert.AreEqual(new[] { path }, Directory.GetFiles(temp.DirectoryPath));
    }

    [TestMethod]
    public void WriteAllTextAtomic_ShouldKeepPreviousFileReadable_WhenReplacementFails()
    {
        using TempDirectoryScope temp = new();
        string path = Path.Combine(temp.DirectoryPath, "state.json");
        File.WriteAllText(path, "old");
        PhysicalFileSystem fileSystem = new();

        using FileStream locked = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        Assert.ThrowsException<IOException>(() => fileSystem.WriteAllTextAtomic(path, "new"));

        using StreamReader reader = new(locked, leaveOpen: true);
        Assert.AreEqual("old", reader.ReadToEnd());
        CollectionAssert.AreEqual(new[] { path }, Directory.GetFiles(temp.DirectoryPath));
    }

    [TestMethod]
    public void TryWriteAllTextNew_ShouldCreateNewFile()
    {
        using TempDirectoryScope temp = new();
        string path = Path.Combine(temp.DirectoryPath, "owner.json");
        PhysicalFileSystem fileSystem = new();

        bool created = fileSystem.TryWriteAllTextNew(path, "new owner");

        Assert.IsTrue(created);
        Assert.AreEqual("new owner", File.ReadAllText(path));
    }

    [TestMethod]
    public void TryWriteAllTextNew_ShouldNotOverwriteExistingFile()
    {
        using TempDirectoryScope temp = new();
        string path = Path.Combine(temp.DirectoryPath, "owner.json");
        File.WriteAllText(path, "foreign owner");
        PhysicalFileSystem fileSystem = new();

        bool created = fileSystem.TryWriteAllTextNew(path, "new owner");

        Assert.IsFalse(created);
        Assert.AreEqual("foreign owner", File.ReadAllText(path));
    }

    [TestMethod]
    public void MoveFile_ShouldOverwriteDestination_WhenRequested()
    {
        using TempDirectoryScope temp = new();
        string sourcePath = Path.Combine(temp.DirectoryPath, "source.txt");
        string destinationPath = Path.Combine(temp.DirectoryPath, "destination.txt");
        File.WriteAllText(sourcePath, "new");
        File.WriteAllText(destinationPath, "old");
        PhysicalFileSystem fileSystem = new();

        fileSystem.MoveFile(sourcePath, destinationPath, overwrite: true);

        Assert.IsFalse(File.Exists(sourcePath));
        Assert.AreEqual("new", File.ReadAllText(destinationPath));
    }

    [TestMethod]
    public void MoveDirectory_ShouldCopyThenRemoveSource_WhenVolumesDiffer()
    {
        string destinationRoot = Path.GetPathRoot(Path.GetTempPath()) ?? throw new InvalidOperationException("Temp root is required.");
        string? sourceRoot = Directory.GetLogicalDrives()
            .FirstOrDefault(root => !string.Equals(root, destinationRoot, StringComparison.OrdinalIgnoreCase) && IsWritable(root));
        if (sourceRoot == null)
        {
            Assert.Inconclusive("A second writable volume is required to exercise cross-volume directory moves.");
        }

        string id = Guid.NewGuid().ToString("N");
        string sourceContainer = Path.Combine(sourceRoot, "RS.SetupApp-Tests", id);
        string sourceDirectory = Path.Combine(sourceContainer, "source");
        string destinationContainer = Path.Combine(Path.GetTempPath(), "RS.SetupApp-Tests", id);
        string destinationDirectory = Path.Combine(destinationContainer, "destination");
        try
        {
            Directory.CreateDirectory(sourceDirectory);
            File.WriteAllText(Path.Combine(sourceDirectory, "payload.txt"), "fixture");

            new PhysicalFileSystem().MoveDirectory(sourceDirectory, destinationDirectory);

            Assert.IsFalse(Directory.Exists(sourceDirectory));
            Assert.AreEqual("fixture", File.ReadAllText(Path.Combine(destinationDirectory, "payload.txt")));
        }
        finally
        {
            if (Directory.Exists(sourceContainer))
            {
                Directory.Delete(sourceContainer, recursive: true);
            }

            if (Directory.Exists(destinationContainer))
            {
                Directory.Delete(destinationContainer, recursive: true);
            }
        }
    }

    private static bool IsWritable(string root)
    {
        try
        {
            string probe = Path.Combine(root, $".rs-setup-write-probe-{Guid.NewGuid():N}");
            Directory.CreateDirectory(probe);
            Directory.Delete(probe);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
