using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Fakes;

public sealed class FaultInjectingFileSystem(IFileSystem inner) : IFileSystem
{
    public List<(string Operation, string Path)> Mutations { get; } = new();

    public Func<string, string, Exception?>? FailureFactory { get; set; }

    public bool FileExists(string path) => inner.FileExists(path);

    public bool DirectoryExists(string path) => inner.DirectoryExists(path);

    public FileAttributes GetAttributes(string path) => inner.GetAttributes(path);

    public void CreateDirectory(string path) => inner.CreateDirectory(path);

    public void DeleteDirectory(string path, bool recursive)
    {
        ThrowIfRequested(nameof(DeleteDirectory), path);
        inner.DeleteDirectory(path, recursive);
    }

    public void DeleteFile(string path)
    {
        ThrowIfRequested(nameof(DeleteFile), path);
        inner.DeleteFile(path);
    }

    public void MoveDirectory(string sourceDirectoryName, string destDirectoryName) => inner.MoveDirectory(sourceDirectoryName, destDirectoryName);

    public void MoveFile(string sourceFileName, string destFileName, bool overwrite) => inner.MoveFile(sourceFileName, destFileName, overwrite);

    public void CopyDirectory(string sourceDirectoryName, string destDirectoryName, bool overwrite) => inner.CopyDirectory(sourceDirectoryName, destDirectoryName, overwrite);

    public void CopyFile(string sourceFileName, string destFileName, bool overwrite) => inner.CopyFile(sourceFileName, destFileName, overwrite);

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) => inner.EnumerateFiles(path, searchPattern, searchOption);

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption) => inner.EnumerateDirectories(path, searchPattern, searchOption);

    public Stream OpenRead(string path) => inner.OpenRead(path);

    public void WriteAllText(string path, string contents) => inner.WriteAllText(path, contents);

    public void WriteAllTextAtomic(string path, string contents)
    {
        Exception? exception = FailureFactory?.Invoke(nameof(WriteAllTextAtomic), path);
        if (exception != null)
        {
            throw exception;
        }

        Mutations.Add((nameof(WriteAllTextAtomic), path));
        inner.WriteAllTextAtomic(path, contents);
    }

    public bool TryWriteAllTextNew(string path, string contents) => inner.TryWriteAllTextNew(path, contents);

    public string ReadAllText(string path) => inner.ReadAllText(path);

    public long GetFileLength(string path) => inner.GetFileLength(path);

    private void ThrowIfRequested(string operation, string path)
    {
        Exception? exception = FailureFactory?.Invoke(operation, path);
        if (exception != null)
        {
            throw exception;
        }
    }
}
