using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Fakes;

public sealed class FaultingFileSystem(IFileSystem inner) : IFileSystem
{
    public List<(string Operation, string Path)> Mutations { get; } = new();

    public Func<string, string, Exception?>? FailureFactory { get; set; }

    public Func<string, bool?>? FileExistsOverride { get; set; }

    public Func<string, bool?>? DirectoryExistsOverride { get; set; }

    public bool FileExists(string path)
    {
        ThrowIfRequested(nameof(FileExists), path);
        bool? overridden = FileExistsOverride?.Invoke(path);
        if (overridden.HasValue)
        {
            return overridden.Value;
        }

        return inner.FileExists(path);
    }

    public bool DirectoryExists(string path)
    {
        ThrowIfRequested(nameof(DirectoryExists), path);
        bool? overridden = DirectoryExistsOverride?.Invoke(path);
        if (overridden.HasValue)
        {
            return overridden.Value;
        }

        return inner.DirectoryExists(path);
    }

    public FileAttributes GetAttributes(string path)
    {
        ThrowIfRequested(nameof(GetAttributes), path);
        return inner.GetAttributes(path);
    }

    public void CreateDirectory(string path)
    {
        ThrowIfRequested(nameof(CreateDirectory), path);
        Mutations.Add((nameof(CreateDirectory), path));
        inner.CreateDirectory(path);
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        ThrowIfRequested(nameof(DeleteDirectory), path);
        Mutations.Add((nameof(DeleteDirectory), path));
        inner.DeleteDirectory(path, recursive);
    }

    public void DeleteFile(string path)
    {
        ThrowIfRequested(nameof(DeleteFile), path);
        Mutations.Add((nameof(DeleteFile), path));
        inner.DeleteFile(path);
    }

    public void MoveDirectory(string sourceDirectoryName, string destDirectoryName)
    {
        ThrowIfRequested(nameof(MoveDirectory), sourceDirectoryName);
        Mutations.Add((nameof(MoveDirectory), sourceDirectoryName));
        inner.MoveDirectory(sourceDirectoryName, destDirectoryName);
    }

    public void MoveFile(string sourceFileName, string destFileName, bool overwrite)
    {
        ThrowIfRequested(nameof(MoveFile), destFileName);
        Mutations.Add((nameof(MoveFile), destFileName));
        inner.MoveFile(sourceFileName, destFileName, overwrite);
    }

    public void CopyDirectory(string sourceDirectoryName, string destDirectoryName, bool overwrite)
    {
        ThrowIfRequested(nameof(CopyDirectory), destDirectoryName);
        Mutations.Add((nameof(CopyDirectory), destDirectoryName));
        inner.CopyDirectory(sourceDirectoryName, destDirectoryName, overwrite);
    }

    public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
    {
        ThrowIfRequested(nameof(CopyFile), destFileName);
        Mutations.Add((nameof(CopyFile), destFileName));
        inner.CopyFile(sourceFileName, destFileName, overwrite);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        ThrowIfRequested(nameof(EnumerateFiles), path);
        return inner.EnumerateFiles(path, searchPattern, searchOption);
    }

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        ThrowIfRequested(nameof(EnumerateDirectories), path);
        return inner.EnumerateDirectories(path, searchPattern, searchOption);
    }

    public Stream OpenRead(string path)
    {
        ThrowIfRequested(nameof(OpenRead), path);
        return inner.OpenRead(path);
    }

    public void WriteAllText(string path, string contents)
    {
        ThrowIfRequested(nameof(WriteAllText), path);
        Mutations.Add((nameof(WriteAllText), path));
        inner.WriteAllText(path, contents);
    }

    public void WriteAllTextAtomic(string path, string contents)
    {
        ThrowIfRequested(nameof(WriteAllTextAtomic), path);
        Mutations.Add((nameof(WriteAllTextAtomic), path));
        inner.WriteAllTextAtomic(path, contents);
    }

    public bool TryWriteAllTextNew(string path, string contents)
    {
        ThrowIfRequested(nameof(TryWriteAllTextNew), path);
        Mutations.Add((nameof(TryWriteAllTextNew), path));
        return inner.TryWriteAllTextNew(path, contents);
    }

    public string ReadAllText(string path)
    {
        ThrowIfRequested(nameof(ReadAllText), path);
        return inner.ReadAllText(path);
    }

    public long GetFileLength(string path)
    {
        ThrowIfRequested(nameof(GetFileLength), path);
        return inner.GetFileLength(path);
    }

    private void ThrowIfRequested(string operation, string path)
    {
        Exception? exception = FailureFactory?.Invoke(operation, path);
        if (exception != null)
        {
            throw exception;
        }
    }
}
