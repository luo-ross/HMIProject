namespace RS.SetupApp.Tests.Helpers;

public sealed class TempDirectoryScope : IDisposable
{
    public TempDirectoryScope()
    {
        DirectoryPath = Path.Combine(Path.GetTempPath(), "RS.SetupApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DirectoryPath);
    }

    public string DirectoryPath { get; }

    public void Dispose()
    {
        if (Directory.Exists(DirectoryPath))
        {
            Directory.Delete(DirectoryPath, recursive: true);
        }
    }
}
