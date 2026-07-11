using System.Windows;

namespace RS.SetupApp.Services;

public interface ISetupDialogService
{
    Task<bool> ConfirmCancellationAsync();

    void ShowError(string message, string title);
}

public sealed class SetupDialogService : ISetupDialogService
{
    public Task<bool> ConfirmCancellationAsync()
    {
        return Task.FromResult(MessageBox.Show(
            "Setup will finish rollback or recovery before it can close. Cancel the current operation?",
            "Setup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes);
    }

    public void ShowError(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

public sealed class NoopSetupDialogService : ISetupDialogService
{
    public Task<bool> ConfirmCancellationAsync() => Task.FromResult(false);

    public void ShowError(string message, string title) { }
}
