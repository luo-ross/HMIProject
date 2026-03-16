using System.Text.Json;

namespace RS.SetupApp.Core;

public sealed class FileSetupLogger : ISetupLogger
{
    private readonly StreamWriter _writer;

    public FileSetupLogger(string logPath)
    {
        LogPath = logPath;
        string? directory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(File.Open(logPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true
        };
    }

    public string LogPath { get; }

    public void Dispose()
    {
        _writer.Dispose();
    }

    public void Info(string message, object? data = null) => Write("info", message, null, data);

    public void Warn(string message, object? data = null) => Write("warn", message, null, data);

    public void Error(string message, Exception? exception = null, object? data = null) => Write("error", message, exception, data);

    private void Write(string level, string message, Exception? exception, object? data)
    {
        string line = JsonSerializer.Serialize(new
        {
            timeUtc = DateTimeOffset.UtcNow,
            level,
            message,
            exception = exception?.ToString(),
            data
        });
        _writer.WriteLine(line);
    }
}
