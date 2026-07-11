using System.Collections.ObjectModel;

namespace RS.SetupApp.ViewModels;

public sealed class OperationProgressViewModel : ObservableObject
{
    private double _percent;
    private string _currentStep = string.Empty;
    private string? _logPath;
    private bool _isLogExpanded;

    public ObservableCollection<string> CompletedSteps { get; } = [];

    public double Percent
    {
        get => _percent;
        set => SetProperty(ref _percent, value);
    }

    public string CurrentStep
    {
        get => _currentStep;
        set => SetProperty(ref _currentStep, value);
    }

    public string? LogPath
    {
        get => _logPath;
        set => SetProperty(ref _logPath, value);
    }

    public bool IsLogExpanded
    {
        get => _isLogExpanded;
        set => SetProperty(ref _isLogExpanded, value);
    }

    public void Reset(string initialStep)
    {
        Percent = 0;
        CurrentStep = initialStep;
        LogPath = null;
        IsLogExpanded = false;
        CompletedSteps.Clear();
    }

    public void Report(string step, double percent)
    {
        Percent = percent;
        CurrentStep = step;
        if (!string.IsNullOrWhiteSpace(step) && !CompletedSteps.Contains(step))
        {
            CompletedSteps.Add(step);
        }
    }
}
