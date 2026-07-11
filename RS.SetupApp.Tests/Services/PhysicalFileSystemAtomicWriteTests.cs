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
}
