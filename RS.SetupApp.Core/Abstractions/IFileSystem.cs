namespace RS.SetupApp.Core;

public interface IFileSystem
{
    bool FileExists(string path);

    bool DirectoryExists(string path);

    void CreateDirectory(string path);

    void DeleteDirectory(string path, bool recursive);

    void DeleteFile(string path);

    void MoveDirectory(string sourceDirectoryName, string destDirectoryName);

    void CopyDirectory(string sourceDirectoryName, string destDirectoryName, bool overwrite);

    void CopyFile(string sourceFileName, string destFileName, bool overwrite);

    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

    Stream OpenRead(string path);

    void WriteAllText(string path, string contents);

    string ReadAllText(string path);

    long GetFileLength(string path);
}
