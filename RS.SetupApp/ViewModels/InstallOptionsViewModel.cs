namespace RS.SetupApp.ViewModels;

public sealed class InstallOptionsViewModel : ObservableObject
{
    private string _installDirectory = string.Empty;
    private bool _installForAllUsers;
    private bool _createShortcuts = true;
    private bool _enableAutoStart;
    private bool _purgeData;
    private bool _allowMachineInstall = true;
    private bool _isLocked;

    public string InstallDirectory
    {
        get => _installDirectory;
        set => SetProperty(ref _installDirectory, value);
    }

    public bool InstallForAllUsers
    {
        get => _installForAllUsers;
        set
        {
            if (!AllowMachineInstall && value)
            {
                return;
            }

            SetProperty(ref _installForAllUsers, value);
        }
    }

    public bool CreateShortcuts
    {
        get => _createShortcuts;
        set => SetProperty(ref _createShortcuts, value);
    }

    public bool EnableAutoStart
    {
        get => _enableAutoStart;
        set => SetProperty(ref _enableAutoStart, value);
    }

    public bool PurgeData
    {
        get => _purgeData;
        set => SetProperty(ref _purgeData, value);
    }

    public bool AllowMachineInstall
    {
        get => _allowMachineInstall;
        set => SetProperty(ref _allowMachineInstall, value);
    }

    public bool IsLocked
    {
        get => _isLocked;
        set => SetProperty(ref _isLocked, value);
    }

    public bool CanChangeInstallDirectory => !IsLocked;
}
