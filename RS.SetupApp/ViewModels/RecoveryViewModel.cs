using System.Collections.ObjectModel;

namespace RS.SetupApp.ViewModels;

public sealed class RecoveryViewModel : ObservableObject
{
    private string _message = string.Empty;
    private string? _logPath;
    private AsyncCommand? _retryCommand;

    public ObservableCollection<string> Errors { get; } = [];

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string? LogPath
    {
        get => _logPath;
        set => SetProperty(ref _logPath, value);
    }

    public AsyncCommand RetryCommand
    {
        get => _retryCommand ?? throw new InvalidOperationException("The recovery command has not been initialized.");
        set => SetProperty(ref _retryCommand, value);
    }

    public void Show(string message, IEnumerable<string> errors, string? logPath)
    {
        Message = message;
        LogPath = logPath;
        Errors.Clear();
        foreach (string error in errors)
        {
            Errors.Add(error);
        }
    }
}
