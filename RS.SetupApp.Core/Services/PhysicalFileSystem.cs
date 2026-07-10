using System.Text;

namespace RS.SetupApp.Core;

public sealed class PhysicalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public FileAttributes GetAttributes(string path) => File.GetAttributes(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void DeleteDirectory(string path, bool recursive)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
    }

    public void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public void MoveDirectory(string sourceDirectoryName, string destDirectoryName)
    {
        Directory.Move(sourceDirectoryName, destDirectoryName);
    }

    public void MoveFile(string sourceFileName, string destFileName, bool overwrite)
    {
        string? directory = Path.GetDirectoryName(destFileName);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Move(sourceFileName, destFileName, overwrite);
    }

    public void CopyDirectory(string sourceDirectoryName, string destDirectoryName, bool overwrite)
    {
        DirectoryInfo source = new(sourceDirectoryName);
        if (!source.Exists)
        {
            throw new DirectoryNotFoundException(sourceDirectoryName);
        }

        Directory.CreateDirectory(destDirectoryName);
        foreach (FileInfo file in source.EnumerateFiles())
        {
            string destination = Path.Combine(destDirectoryName, file.Name);
            file.CopyTo(destination, overwrite);
        }

        foreach (DirectoryInfo child in source.EnumerateDirectories())
        {
            CopyDirectory(child.FullName, Path.Combine(destDirectoryName, child.Name), overwrite);
        }
    }

    public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
    {
        string? directory = Path.GetDirectoryName(destFileName);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(sourceFileName, destFileName, overwrite);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        => Directory.EnumerateFiles(path, searchPattern, searchOption);

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        => Directory.EnumerateDirectories(path, searchPattern, searchOption);

    public Stream OpenRead(string path) => File.OpenRead(path);

    public void WriteAllText(string path, string contents)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, contents, Encoding.UTF8);
    }

    public void WriteAllTextAtomic(string path, string contents)
    {
        string fullPath = Path.GetFullPath(path);
        string directory = Path.GetDirectoryName(fullPath)
            ?? throw new ArgumentException("The path must include a directory.", nameof(path));
        Directory.CreateDirectory(directory);

        string temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            using (FileStream stream = new(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                FileOptions.WriteThrough))
            using (StreamWriter writer = new(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true))
            {
                writer.Write(contents);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(fullPath))
            {
                File.Replace(temporaryPath, fullPath, destinationBackupFileName: null, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(temporaryPath, fullPath);
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public string ReadAllText(string path) => File.ReadAllText(path, Encoding.UTF8);

    public long GetFileLength(string path) => new FileInfo(path).Length;
}
