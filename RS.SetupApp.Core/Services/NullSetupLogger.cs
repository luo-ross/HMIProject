namespace RS.SetupApp.Core;

public sealed class NullSetupLogger : ISetupLogger
{
    public string LogPath => string.Empty;

    public void Dispose()
    {
    }

    public void Info(string message, object? data = null)
    {
    }

    public void Warn(string message, object? data = null)
    {
    }

    public void Error(string message, Exception? exception = null, object? data = null)
    {
    }
}
