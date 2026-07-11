namespace RS.SetupApp.ViewModels;

public sealed class MaintenanceViewModel : ObservableObject
{
    private bool _isInstalled;
    private string _installedVersion = string.Empty;
    private string _canonicalInstallRoot = string.Empty;
    private bool _hasLegacyInstallationToClaim;
    private AsyncCommand? _claimLegacyInstallationCommand;

    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }

    public string InstalledVersion
    {
        get => _installedVersion;
        set => SetProperty(ref _installedVersion, value);
    }

    public string CanonicalInstallRoot
    {
        get => _canonicalInstallRoot;
        set => SetProperty(ref _canonicalInstallRoot, value);
    }

    public bool HasLegacyInstallationToClaim
    {
        get => _hasLegacyInstallationToClaim;
        set => SetProperty(ref _hasLegacyInstallationToClaim, value);
    }

    public AsyncCommand ClaimLegacyInstallationCommand
    {
        get => _claimLegacyInstallationCommand ?? throw new InvalidOperationException("The maintenance command has not been initialized.");
        set => SetProperty(ref _claimLegacyInstallationCommand, value);
    }
}
