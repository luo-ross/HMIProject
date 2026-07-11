using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RS.SetupApp.Core;
using RS.SetupApp.Services;

namespace RS.SetupApp.ViewModels;

/// <summary>
/// Owns presentation state and cancellation only. The core workflow remains the source of truth for
/// validation, rollback and recovery; this class never closes the process or duplicates that work.
/// </summary>
public sealed class MainWindowViewModel : ObservableObject
{
    private readonly ISetupWorkflow _workflow;
    private readonly ISetupRelaunchService _relaunchService;
    private readonly IFolderPicker _folderPicker;
    private readonly IExternalLauncher _externalLauncher;
    private readonly ISetupDialogService _dialogService;
    private readonly RuntimeOptions? _startupOptions;
    private readonly object _closeGate = new();

    private CancellationTokenSource? _activeOperationCts;
    private Task<bool>? _closeRequest;
    private SetupWorkspace? _workspace;
    private SetupUiState _uiState = SetupUiState.Idle;
    private WizardPageKind _currentPage = WizardPageKind.Welcome;
    private UiLanguage _selectedLanguage;
    private SetupLanguageResources _ui;
    private string _productName = "Generic Setup";
    private string _publisher = string.Empty;
    private string _welcomeText;
    private string _licenseText = string.Empty;
    private string _statusMessage;
    private string _completionMessage = string.Empty;
    private string _availableUpdateVersion;
    private string _availableUpdateNotes = string.Empty;
    private string? _supportUrl;
    private string? _updateUrl;
    private bool _acceptLicense = true;
    private bool _isInstalled;
    private bool _operationSucceeded;
    private ImageSource? _windowIcon;
    private Brush _accentBrush = Brushes.Teal;

    public MainWindowViewModel(
        ISetupWorkflow workflow,
        ISetupRelaunchService relaunchService,
        IFolderPicker folderPicker,
        IExternalLauncher externalLauncher,
        ISetupDialogService dialogService,
        RuntimeOptions? startupOptions = null)
    {
        _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
        _relaunchService = relaunchService ?? throw new ArgumentNullException(nameof(relaunchService));
        _folderPicker = folderPicker ?? throw new ArgumentNullException(nameof(folderPicker));
        _externalLauncher = externalLauncher ?? throw new ArgumentNullException(nameof(externalLauncher));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _startupOptions = startupOptions;
        _selectedLanguage = SetupLanguageCatalog.ResolveDefaultLanguage();
        _ui = SetupLanguageCatalog.Get(_selectedLanguage);
        _welcomeText = string.Format(_ui.DefaultWelcomeTemplate, _productName);
        _statusMessage = _ui.ReadyStatusText;
        _availableUpdateVersion = _ui.NotCheckedStatusText;

        InstallOptions = new InstallOptionsViewModel();
        Progress = new OperationProgressViewModel();
        Maintenance = new MaintenanceViewModel();
        Recovery = new RecoveryViewModel();

        ContinueCommand = new AsyncCommand(_ => NavigateFromWelcomeAsync(), ReportCommandError);
        BackCommand = new AsyncCommand(_ => NavigateBackAsync(), ReportCommandError);
        ReviewCommand = new AsyncCommand(_ => NavigateToReviewAsync(), ReportCommandError, () => CanExecuteAction);
        OpenInstallOptionsCommand = new AsyncCommand(_ => NavigateToInstallOptionsAsync(), ReportCommandError, () => !IsBusy);
        ShowUninstallConfirmationCommand = new AsyncCommand(_ => NavigateToUninstallConfirmationAsync(), ReportCommandError, () => !IsBusy && IsInstalled);
        FinishCommand = new AsyncCommand(_ => RequestFinishAsync(), ReportCommandError, () => IsCloseAllowed);
        InstallCommand = new AsyncCommand(_ => ExecuteSelectedAsync(SetupMode.Install), ReportCommandError, () => CanExecuteAction);
        RepairCommand = new AsyncCommand(_ => ExecuteSelectedAsync(SetupMode.Repair), ReportCommandError, () => !IsBusy && IsInstalled);
        UpdateCommand = new AsyncCommand(_ => ExecuteSelectedAsync(SetupMode.Update), ReportCommandError, () => !IsBusy && IsInstalled);
        UninstallCommand = new AsyncCommand(_ => ExecuteSelectedAsync(SetupMode.Uninstall), ReportCommandError, () => !IsBusy && IsInstalled);
        BrowseInstallDirectoryCommand = new AsyncCommand(_ => BrowseInstallDirectoryAsync(), ReportCommandError, () => InstallOptions.CanChangeInstallDirectory && !IsBusy);
        ResetInstallDirectoryCommand = new AsyncCommand(_ => ResetInstallDirectoryAsync(), ReportCommandError, () => InstallOptions.CanChangeInstallDirectory && !IsBusy);
        CheckForUpdatesCommand = new AsyncCommand(_ => CheckForUpdatesAsync(), ReportCommandError, () => !IsBusy && IsInstalled);
        CancelCommand = new AsyncCommand(_ => RequestCancelAsync(), ReportCommandError, () => IsBusy && UiState is SetupUiState.Preparing or SetupUiState.Running);
        OpenLogCommand = new AsyncCommand(_ => OpenLogAsync(), ReportCommandError, () => !string.IsNullOrWhiteSpace(Progress.LogPath ?? Recovery.LogPath));
        LaunchInstalledApplicationCommand = new AsyncCommand(_ => LaunchInstalledApplicationAsync(), ReportCommandError, () => CanLaunchInstalledApplication);
        Maintenance.ClaimLegacyInstallationCommand = new AsyncCommand(
            _ => ClaimLegacyInstallationAsync(),
            ReportCommandError,
            () => Maintenance.HasLegacyInstallationToClaim && !IsBusy);
        Recovery.RetryCommand = new AsyncCommand(_ => RecoverAsync(), ReportCommandError, () => UiState == SetupUiState.RecoveryFailed);
    }

    public event EventHandler? RelaunchRequested;

    public event EventHandler? FinishRequested;

    public SetupUiState UiState
    {
        get => _uiState;
        private set
        {
            if (SetProperty(ref _uiState, value))
            {
                RaisePropertyChanged(nameof(IsBusy));
                RaisePropertyChanged(nameof(IsCloseAllowed));
                RaisePropertyChanged(nameof(CanExecuteAction));
                RaisePropertyChanged(nameof(CancelLabel));
                RefreshCommandAvailability();
            }
        }
    }

    public WizardPageKind CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public InstallOptionsViewModel InstallOptions { get; }

    public OperationProgressViewModel Progress { get; }

    public MaintenanceViewModel Maintenance { get; }

    public RecoveryViewModel Recovery { get; }

    public AsyncCommand ContinueCommand { get; }

    public AsyncCommand BackCommand { get; }

    public AsyncCommand ReviewCommand { get; }

    public AsyncCommand OpenInstallOptionsCommand { get; }

    public AsyncCommand ShowUninstallConfirmationCommand { get; }

    public AsyncCommand FinishCommand { get; }

    public AsyncCommand InstallCommand { get; }

    public AsyncCommand RepairCommand { get; }

    public AsyncCommand UpdateCommand { get; }

    public AsyncCommand UninstallCommand { get; }

    public AsyncCommand BrowseInstallDirectoryCommand { get; }

    public AsyncCommand ResetInstallDirectoryCommand { get; }

    public AsyncCommand CheckForUpdatesCommand { get; }

    public AsyncCommand CancelCommand { get; }

    public AsyncCommand OpenLogCommand { get; }

    public AsyncCommand LaunchInstalledApplicationCommand { get; }

    public UiLanguage SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value))
            {
                ApplyLanguage();
            }
        }
    }

    public bool IsEnglishSelected
    {
        get => SelectedLanguage == UiLanguage.English;
        set { if (value) { SelectedLanguage = UiLanguage.English; } }
    }

    public bool IsChineseSelected
    {
        get => SelectedLanguage == UiLanguage.ChineseSimplified;
        set { if (value) { SelectedLanguage = UiLanguage.ChineseSimplified; } }
    }

    public SetupLanguageResources Ui
    {
        get => _ui;
        private set => SetProperty(ref _ui, value);
    }

    public string ProductName
    {
        get => _productName;
        private set => SetProperty(ref _productName, value);
    }

    public string Publisher
    {
        get => _publisher;
        private set => SetProperty(ref _publisher, value);
    }

    public string WelcomeText
    {
        get => _welcomeText;
        private set => SetProperty(ref _welcomeText, value);
    }

    public string LicenseText
    {
        get => _licenseText;
        private set
        {
            if (SetProperty(ref _licenseText, value))
            {
                RaisePropertyChanged(nameof(HasLicense));
                RaisePropertyChanged(nameof(CanExecuteAction));
                RefreshCommandAvailability();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string CompletionMessage
    {
        get => _completionMessage;
        private set => SetProperty(ref _completionMessage, value);
    }

    public string AvailableUpdateVersion
    {
        get => _availableUpdateVersion;
        private set => SetProperty(ref _availableUpdateVersion, value);
    }

    public string AvailableUpdateNotes
    {
        get => _availableUpdateNotes;
        private set => SetProperty(ref _availableUpdateNotes, value);
    }

    public bool AcceptLicense
    {
        get => _acceptLicense;
        set
        {
            if (SetProperty(ref _acceptLicense, value))
            {
                RaisePropertyChanged(nameof(CanExecuteAction));
                RefreshCommandAvailability();
            }
        }
    }

    public bool HasLicense => !string.IsNullOrWhiteSpace(LicenseText);

    public bool IsInstalled
    {
        get => _isInstalled;
        private set
        {
            if (SetProperty(ref _isInstalled, value))
            {
                RaisePropertyChanged(nameof(CanLaunchInstalledApplication));
                RefreshCommandAvailability();
            }
        }
    }

    public bool OperationSucceeded
    {
        get => _operationSucceeded;
        private set => SetProperty(ref _operationSucceeded, value);
    }

    public bool IsBusy => UiState is SetupUiState.Preparing or SetupUiState.Running or SetupUiState.CancellationRequested or SetupUiState.RollingBack;

    public bool IsCloseAllowed => UiState is SetupUiState.Idle or SetupUiState.Succeeded or SetupUiState.Failed or SetupUiState.RecoveryFailed;

    public bool CanExecuteAction => !IsBusy && (!HasLicense || AcceptLicense);

    public bool CanLaunchInstalledApplication => IsInstalled && _workspace?.InstalledState is { } state && File.Exists(state.MainExecutablePath);

    public bool ShouldAutoRunStartupOperation => _startupOptions != null;

    public bool HasSupportLink => !string.IsNullOrWhiteSpace(_supportUrl);

    public bool HasUpdateLink => !string.IsNullOrWhiteSpace(_updateUrl);

    public string CancelLabel => UiState switch
    {
        SetupUiState.CancellationRequested => "Cancellation requested — waiting for the setup engine.",
        SetupUiState.RollingBack => "Recovery in progress — this window will remain open.",
        _ => "Cancel"
    };

    public ImageSource? WindowIcon
    {
        get => _windowIcon;
        private set => SetProperty(ref _windowIcon, value);
    }

    public Brush AccentBrush
    {
        get => _accentBrush;
        private set => SetProperty(ref _accentBrush, value);
    }

    public async Task InitializeAsync()
    {
        SetupWorkspace workspace = await _workflow.LoadAsync(CancellationToken.None).ConfigureAwait(true);
        ApplyWorkspace(workspace);
        UiState = SetupUiState.Idle;
        CurrentPage = IsInstalled ? WizardPageKind.Maintenance : WizardPageKind.Welcome;
    }

    public Task RunStartupOperationAsync()
    {
        return _startupOptions == null ? Task.CompletedTask : ExecuteAsync(_startupOptions);
    }

    public async Task ExecuteAsync(RuntimeOptions options)
    {
        if (IsBusy)
        {
            return;
        }

        using CancellationTokenSource operationCts = new();
        _activeOperationCts = operationCts;
        UiState = SetupUiState.Preparing;
        CurrentPage = WizardPageKind.Progress;
        OperationSucceeded = false;
        Progress.Reset($"Preparing {options.Mode.ToString().ToLowerInvariant()}…");
        StatusMessage = Progress.CurrentStep;
        UiState = SetupUiState.Running;

        Progress<SetupProgress> progress = new(ReportProgress);

        try
        {
            SetupOperationResult result = await _workflow
                .ExecuteAsync(options, progress, operationCts.Token)
                .ConfigureAwait(true);
            ApplyOperationResult(result);
        }
        catch (OperationCanceledException)
        {
            ApplyOperationResult(new SetupOperationResult
            {
                Status = SetupOperationStatus.Cancelled,
                Message = "Cancelled safely.",
                Mode = options.Mode
            });
        }
        finally
        {
            if (ReferenceEquals(_activeOperationCts, operationCts))
            {
                _activeOperationCts = null;
            }
        }
    }

    public Task RequestCancelAsync()
    {
        if (UiState is SetupUiState.Preparing or SetupUiState.Running)
        {
            UiState = SetupUiState.CancellationRequested;
            StatusMessage = "Cancellation requested. Waiting for rollback-safe completion.";
            _activeOperationCts?.Cancel();
        }

        return Task.CompletedTask;
    }

    public Task<bool> RequestCloseAsync() => RequestCloseAsync(_dialogService.ConfirmCancellationAsync);

    public Task<bool> RequestCloseAsync(Func<Task<bool>> confirmCancellationAsync)
    {
        ArgumentNullException.ThrowIfNull(confirmCancellationAsync);
        if (IsCloseAllowed)
        {
            return Task.FromResult(true);
        }

        lock (_closeGate)
        {
            return _closeRequest ??= RequestCloseCoreAsync(confirmCancellationAsync);
        }
    }

    public async Task RecoverAsync()
    {
        if (UiState != SetupUiState.RecoveryFailed)
        {
            return;
        }

        using CancellationTokenSource recoveryCts = new();
        _activeOperationCts = recoveryCts;
        UiState = SetupUiState.RollingBack;
        CurrentPage = WizardPageKind.Recovery;
        StatusMessage = "Retrying recovery…";
        try
        {
            SetupOperationResult result = await _workflow.RecoverAsync(recoveryCts.Token).ConfigureAwait(true);
            ApplyOperationResult(result);
        }
        catch (OperationCanceledException)
        {
            ApplyOperationResult(new SetupOperationResult { Status = SetupOperationStatus.Cancelled, Message = "Recovery cancellation completed." });
        }
        finally
        {
            if (ReferenceEquals(_activeOperationCts, recoveryCts))
            {
                _activeOperationCts = null;
            }
        }
    }

    public async Task CheckForUpdatesAsync()
    {
        if (_workspace == null || IsBusy)
        {
            return;
        }

        try
        {
            UpdateFeedManifest? update = await _workflow
                .CheckForUpdatesAsync(_workspace.ProductManifestPath, CancellationToken.None)
                .ConfigureAwait(true);
            AvailableUpdateVersion = update?.Version ?? Ui.NoUpdatesStatusText;
            AvailableUpdateNotes = update?.ReleaseNotes ?? string.Empty;
            StatusMessage = update == null ? Ui.NoUpdateAvailableStatusText : Ui.FormatAvailableVersion(update.Version);
        }
        catch (Exception exception)
        {
            AvailableUpdateVersion = Ui.CheckFailedStatusText;
            AvailableUpdateNotes = exception.Message;
            StatusMessage = Ui.CheckFailedStatusText;
        }
    }

    public void OpenSupportLink()
    {
        if (!string.IsNullOrWhiteSpace(_supportUrl))
        {
            _externalLauncher.LaunchUrl(_supportUrl);
        }
    }

    public void OpenUpdateLink()
    {
        if (!string.IsNullOrWhiteSpace(_updateUrl))
        {
            _externalLauncher.LaunchUrl(_updateUrl);
        }
    }

    public void ReportUnexpectedError(Exception exception) => ReportCommandError(exception);

    private async Task<bool> RequestCloseCoreAsync(Func<Task<bool>> confirmCancellationAsync)
    {
        // Defer the prompt so the coordinating task is stored before a synchronous dialog fake can complete.
        await Task.Yield();
        if (UiState is not (SetupUiState.Preparing or SetupUiState.Running))
        {
            return false;
        }

        if (!await confirmCancellationAsync().ConfigureAwait(true))
        {
            lock (_closeGate)
            {
                _closeRequest = null;
            }
            return false;
        }

        await RequestCancelAsync().ConfigureAwait(true);
        return false;
    }

    private Task NavigateFromWelcomeAsync()
    {
        CurrentPage = HasLicense ? WizardPageKind.License : WizardPageKind.InstallOptions;
        return Task.CompletedTask;
    }

    private Task NavigateBackAsync()
    {
        CurrentPage = CurrentPage switch
        {
            WizardPageKind.License => WizardPageKind.Welcome,
            WizardPageKind.InstallOptions => IsInstalled ? WizardPageKind.Maintenance : WizardPageKind.Welcome,
            WizardPageKind.Review => WizardPageKind.InstallOptions,
            WizardPageKind.UninstallConfirm => WizardPageKind.Maintenance,
            WizardPageKind.Update => WizardPageKind.Maintenance,
            _ => CurrentPage
        };
        return Task.CompletedTask;
    }

    private Task NavigateToReviewAsync()
    {
        if (CanExecuteAction)
        {
            CurrentPage = WizardPageKind.Review;
        }

        return Task.CompletedTask;
    }

    private Task NavigateToInstallOptionsAsync()
    {
        CurrentPage = WizardPageKind.InstallOptions;
        return Task.CompletedTask;
    }

    private Task NavigateToUninstallConfirmationAsync()
    {
        CurrentPage = WizardPageKind.UninstallConfirm;
        return Task.CompletedTask;
    }

    private Task RequestFinishAsync()
    {
        FinishRequested?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    private async Task ExecuteSelectedAsync(SetupMode mode)
    {
        RuntimeOptions options = CreateOptions(mode);
        if (await _relaunchService.TryRelaunchAsync(options, BuildArguments(options), CancellationToken.None).ConfigureAwait(true))
        {
            RelaunchRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        await ExecuteAsync(options).ConfigureAwait(true);
    }

    private async Task BrowseInstallDirectoryAsync()
    {
        string? selected = await _folderPicker
            .PickAsync(InstallOptions.InstallDirectory, Ui.SelectFolderDialogDescription, CancellationToken.None)
            .ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(selected))
        {
            InstallOptions.InstallDirectory = Path.GetFullPath(selected);
        }
    }

    private Task ResetInstallDirectoryAsync()
    {
        if (_workspace != null)
        {
            InstallOptions.InstallDirectory = _workspace.InstalledState?.InstallDirectory ??
                GetDefaultInstallDirectory(_workspace.Product, InstallOptions.InstallForAllUsers);
        }

        return Task.CompletedTask;
    }

    private Task LaunchInstalledApplicationAsync()
    {
        if (_workspace?.InstalledState is { } state && File.Exists(state.MainExecutablePath))
        {
            _externalLauncher.LaunchFile(state.MainExecutablePath, state.InstallDirectory);
        }

        return Task.CompletedTask;
    }

    private Task OpenLogAsync()
    {
        string? logPath = Progress.LogPath ?? Recovery.LogPath;
        if (!string.IsNullOrWhiteSpace(logPath) && File.Exists(logPath))
        {
            _externalLauncher.LaunchFile(logPath);
        }

        return Task.CompletedTask;
    }

    private async Task ClaimLegacyInstallationAsync()
    {
        if (_workspace?.InstalledState is not { } state || !Maintenance.HasLegacyInstallationToClaim || IsBusy)
        {
            return;
        }

        RuntimeOptions options = new()
        {
            Mode = SetupMode.Install,
            Scope = state.InstallScope,
            InstallDirectory = state.InstallDirectory,
            ProductManifestPath = _workspace.ProductManifestPath,
            ClaimLegacyInstallation = true,
            SkipLaunch = true
        };
        if (await _relaunchService.TryRelaunchAsync(options, BuildArguments(options), CancellationToken.None).ConfigureAwait(true))
        {
            RelaunchRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        using CancellationTokenSource claimCts = new();
        _activeOperationCts = claimCts;
        UiState = SetupUiState.Preparing;
        StatusMessage = "Verifying and claiming legacy installation ownership…";
        try
        {
            LegacyInstallationClaimResult result = await _workflow
                .ClaimLegacyInstallationAsync(
                    _workspace.Product,
                    state,
                    options,
                    claimCts.Token)
                .ConfigureAwait(true);
            if (result.Succeeded && result.Claimed)
            {
                ApplyWorkspace(await _workflow.LoadAsync(CancellationToken.None).ConfigureAwait(true));
                StatusMessage = result.Message;
                UiState = SetupUiState.Idle;
            }
            else
            {
                CompletionMessage = result.Message;
                StatusMessage = result.Message;
                UiState = SetupUiState.Failed;
                CurrentPage = WizardPageKind.Completion;
            }
        }
        finally
        {
            if (ReferenceEquals(_activeOperationCts, claimCts))
            {
                _activeOperationCts = null;
            }
        }
    }

    private void ApplyWorkspace(SetupWorkspace workspace)
    {
        _workspace = workspace;
        ProductManifest product = workspace.Product;
        _supportUrl = product.SupportUrl;
        _updateUrl = product.UpdateInfoUrl;
        ProductName = LocalizedManifestTextResolver.Resolve(product.DisplayNameLocalized, product.DisplayName, SelectedLanguage);
        Publisher = product.Publisher;
        WelcomeText = ResolveWelcomeText(product);
        LicenseText = LoadLicenseText(product, workspace.ProductManifestPath);
        AcceptLicense = !HasLicense;
        WindowIcon = LoadWindowIcon(product, workspace.ProductManifestPath);
        AccentBrush = ParseBrush(product.Branding.AccentColor);
        IsInstalled = workspace.InstalledState != null;

        InstallOptions.AllowMachineInstall = product.InstallDefaults.AllowMachineInstall;
        InstallOptions.InstallForAllUsers = workspace.InstalledState?.InstallScope == InstallScope.AllUsers ||
            (workspace.InstalledState == null && product.InstallDefaults.DefaultScope == InstallScope.AllUsers && product.InstallDefaults.AllowMachineInstall);
        InstallOptions.InstallDirectory = workspace.InstalledState?.InstallDirectory ?? GetDefaultInstallDirectory(product, InstallOptions.InstallForAllUsers);
        InstallOptions.CreateShortcuts = product.InstallDefaults.CreateShortcutsByDefault;
        InstallOptions.EnableAutoStart = workspace.InstalledState?.AutorunEnabled ?? product.InstallDefaults.EnableAutoStartByDefault;
        InstallOptions.PurgeData = product.Uninstall.PurgeDataByDefault;
        InstallOptions.IsLocked = workspace.InstalledState != null;

        Maintenance.IsInstalled = IsInstalled;
        Maintenance.InstalledVersion = workspace.InstalledState?.Version ?? Ui.NotInstalledStatusText;
        Maintenance.CanonicalInstallRoot = workspace.InstalledState?.InstallDirectory ?? string.Empty;
        Maintenance.HasLegacyInstallationToClaim = workspace.HasValidUnclaimedLegacyInstallation;
        RaisePropertyChanged(nameof(HasSupportLink));
        RaisePropertyChanged(nameof(HasUpdateLink));
        RaisePropertyChanged(nameof(CanLaunchInstalledApplication));
        RefreshCommandAvailability();
    }

    private void ApplyOperationResult(SetupOperationResult result)
    {
        Progress.LogPath = result.LogPath;
        Recovery.LogPath = result.LogPath;
        CompletionMessage = result.Message;
        StatusMessage = result.Message;
        switch (result.Status)
        {
            case SetupOperationStatus.Succeeded:
                OperationSucceeded = true;
                UiState = SetupUiState.Succeeded;
                Progress.Percent = 100;
                CurrentPage = WizardPageKind.Completion;
                if (result.InstalledState != null)
                {
                    UpdateInstalledState(result.InstalledState);
                }
                else if (result.Mode == SetupMode.Uninstall)
                {
                    IsInstalled = false;
                    Maintenance.IsInstalled = false;
                }
                break;
            case SetupOperationStatus.Cancelled:
                OperationSucceeded = false;
                UiState = SetupUiState.Idle;
                CurrentPage = IsInstalled ? WizardPageKind.Maintenance : WizardPageKind.Review;
                break;
            case SetupOperationStatus.RecoveryFailed:
                OperationSucceeded = false;
                Recovery.Show(result.Message, result.RecoveryErrors, result.LogPath);
                UiState = SetupUiState.RecoveryFailed;
                CurrentPage = WizardPageKind.Recovery;
                break;
            default:
                OperationSucceeded = false;
                UiState = SetupUiState.Failed;
                CurrentPage = WizardPageKind.Completion;
                break;
        }
    }

    private void UpdateInstalledState(InstalledStateManifest state)
    {
        if (_workspace == null)
        {
            return;
        }

        _workspace = _workspace with { InstalledState = state, HasValidUnclaimedLegacyInstallation = false };
        IsInstalled = true;
        Maintenance.IsInstalled = true;
        Maintenance.InstalledVersion = state.Version;
        Maintenance.CanonicalInstallRoot = state.InstallDirectory;
        InstallOptions.InstallDirectory = state.InstallDirectory;
        InstallOptions.IsLocked = true;
        RaisePropertyChanged(nameof(CanLaunchInstalledApplication));
    }

    public RuntimeOptions CreateOptions(SetupMode mode)
    {
        if (_workspace == null)
        {
            throw new InvalidOperationException("The product manifest has not been loaded.");
        }

        return new RuntimeOptions
        {
            Mode = mode,
            Scope = InstallOptions.InstallForAllUsers ? InstallScope.AllUsers : InstallScope.CurrentUser,
            ProductManifestPath = _workspace.ProductManifestPath,
            InstallDirectory = InstallOptions.InstallDirectory,
            NoShortcuts = !InstallOptions.CreateShortcuts,
            NoAutostart = !InstallOptions.EnableAutoStart,
            PurgeData = InstallOptions.PurgeData,
            SkipLaunch = true
        };
    }

    public string[] BuildArguments(RuntimeOptions options)
    {
        List<string> arguments =
        [
            "--mode", options.Mode.ToString().ToLowerInvariant(),
            "--scope", options.Scope == InstallScope.AllUsers ? "machine" : "user",
            "--product", options.ProductManifestPath ?? string.Empty,
            "--skip-launch"
        ];
        if (!string.IsNullOrWhiteSpace(options.InstallDirectory)) { arguments.AddRange(["--install-dir", options.InstallDirectory]); }
        if (options.NoShortcuts) { arguments.Add("--no-shortcuts"); }
        if (options.NoAutostart) { arguments.Add("--no-autostart"); }
        if (options.PurgeData) { arguments.Add("--purge-data"); }
        if (options.ClaimLegacyInstallation) { arguments.Add("--claim-legacy"); }
        if (options.Elevated) { arguments.Add("--elevated"); }
        return arguments.ToArray();
    }

    private void ApplyLanguage()
    {
        Ui = SetupLanguageCatalog.Get(SelectedLanguage);
        RaisePropertyChanged(nameof(IsEnglishSelected));
        RaisePropertyChanged(nameof(IsChineseSelected));
        RaisePropertyChanged(nameof(CancelLabel));
        if (_workspace != null)
        {
            ProductName = LocalizedManifestTextResolver.Resolve(_workspace.Product.DisplayNameLocalized, _workspace.Product.DisplayName, SelectedLanguage);
            WelcomeText = ResolveWelcomeText(_workspace.Product);
            Maintenance.InstalledVersion = _workspace.InstalledState?.Version ?? Ui.NotInstalledStatusText;
        }
    }

    private string ResolveWelcomeText(ProductManifest product)
    {
        string fallback = string.IsNullOrWhiteSpace(product.Branding.WelcomeText)
            ? string.Format(Ui.DefaultWelcomeTemplate, ProductName)
            : product.Branding.WelcomeText;
        return LocalizedManifestTextResolver.Resolve(product.Branding.WelcomeTextLocalized, fallback, SelectedLanguage);
    }

    private static string LoadLicenseText(ProductManifest product, string productManifestPath)
    {
        if (string.IsNullOrWhiteSpace(product.Branding.LicensePath))
        {
            return string.Empty;
        }

        string licensePath = SetupPathUtility.ResolveManifestRelativePath(productManifestPath, product.Branding.LicensePath);
        return File.Exists(licensePath) ? File.ReadAllText(licensePath) : string.Empty;
    }

    private static ImageSource? LoadWindowIcon(ProductManifest product, string productManifestPath)
    {
        if (string.IsNullOrWhiteSpace(product.Branding.IconPath))
        {
            return null;
        }

        string iconPath = SetupPathUtility.ResolveManifestRelativePath(productManifestPath, product.Branding.IconPath);
        try { return File.Exists(iconPath) ? BitmapFrame.Create(new Uri(iconPath, UriKind.Absolute)) : null; }
        catch { return null; }
    }

    private static Brush ParseBrush(string color)
    {
        try { return (Brush)new BrushConverter().ConvertFromString(color)!; }
        catch { return Brushes.Teal; }
    }

    private static bool LooksLikeRecovery(string message)
    {
        return message.Contains("rollback", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("recover", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetDefaultInstallDirectory(ProductManifest product, bool allUsers)
    {
        string root = allUsers
            ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, product.Publisher, product.DisplayName);
    }

    private void ReportCommandError(Exception exception)
    {
        StatusMessage = exception.Message;
        _dialogService.ShowError(exception.Message, Ui.ErrorDialogTitle);
    }

    private void RefreshCommandAvailability()
    {
        foreach (AsyncCommand command in new[]
                 {
                     ReviewCommand, InstallCommand, RepairCommand, UpdateCommand, UninstallCommand,
                     OpenInstallOptionsCommand, ShowUninstallConfirmationCommand, FinishCommand,
                     BrowseInstallDirectoryCommand, ResetInstallDirectoryCommand, CheckForUpdatesCommand,
                     CancelCommand, OpenLogCommand, LaunchInstalledApplicationCommand,
                     Maintenance.ClaimLegacyInstallationCommand, Recovery.RetryCommand
                 })
        {
            command.RaiseCanExecuteChanged();
        }
    }

    private void ReportProgress(SetupProgress step)
    {
        Progress.Report(step.Message, step.Percent);
        StatusMessage = step.Message;
        if (UiState == SetupUiState.CancellationRequested && LooksLikeRecovery(step.Message))
        {
            UiState = SetupUiState.RollingBack;
        }
    }

}
