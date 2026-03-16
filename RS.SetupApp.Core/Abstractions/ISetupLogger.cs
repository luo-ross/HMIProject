namespace RS.SetupApp.Core;

public interface ISetupLogger : IDisposable
{
    string LogPath { get; }

    void Info(string message, object? data = null);

    void Warn(string message, object? data = null);

    void Error(string message, Exception? exception = null, object? data = null);
}
