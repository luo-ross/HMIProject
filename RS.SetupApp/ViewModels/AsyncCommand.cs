using System.Windows.Input;

namespace RS.SetupApp.ViewModels;

/// <summary>Awaitable ICommand that serializes execution and keeps failures in the owning view model.</summary>
public sealed class AsyncCommand : ObservableObject, ICommand
{
    private readonly Func<CancellationToken, Task> _execute;
    private readonly Func<bool>? _canExecute;
    private readonly Action<Exception>? _errorHandler;
    private Task _execution = Task.CompletedTask;
    private bool _isExecuting;

    public AsyncCommand(
        Func<CancellationToken, Task> execute,
        Action<Exception>? errorHandler = null,
        Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _errorHandler = errorHandler;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool IsExecuting
    {
        get => _isExecuting;
        private set => SetProperty(ref _isExecuting, value);
    }

    public Task Execution => _execution;

    public bool CanExecute(object? parameter) => !IsExecuting && (_canExecute?.Invoke() ?? true);

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(null))
        {
            return;
        }

        IsExecuting = true;
        RaiseCanExecuteChanged();
        _execution = ExecuteCoreAsync(cancellationToken);
        await _execution.ConfigureAwait(false);
    }

    public async void Execute(object? parameter)
    {
        await ExecuteAsync();
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    private async Task ExecuteCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _execute(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _errorHandler?.Invoke(exception);
        }
        finally
        {
            IsExecuting = false;
            RaiseCanExecuteChanged();
        }
    }
}
