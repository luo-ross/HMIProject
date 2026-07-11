using System.Diagnostics;

namespace RS.SetupApp.Services;

public interface IExternalLauncher
{
    void LaunchFile(string path, string? workingDirectory = null);

    void LaunchUrl(string url);
}

public sealed class ExternalLauncher : IExternalLauncher
{
    public void LaunchFile(string path, string? workingDirectory = null)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            WorkingDirectory = workingDirectory ?? string.Empty,
            UseShellExecute = true
        });
    }

    public void LaunchUrl(string url) => LaunchFile(url);
}

public sealed class NoopExternalLauncher : IExternalLauncher
{
    public void LaunchFile(string path, string? workingDirectory = null) { }

    public void LaunchUrl(string url) { }
}
