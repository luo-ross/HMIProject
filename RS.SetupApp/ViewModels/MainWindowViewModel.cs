using System.Diagnostics;
using System.IO;
using Brush = System.Windows.Media.Brush;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RS.SetupApp.Core;

namespace RS.SetupApp.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly SetupServices _services;
    private readonly SetupEngine _engine;
    private readonly RuntimeOptions? _startupOptions;

    private ProductManifest? _product;
    private string _productManifestPath = string.Empty;
    private InstalledStateManifest? _installedState;
    private WizardPageKind _currentPage = WizardPageKind.Welcome;
    private ImageSource? _windowIcon;
    private UiLanguage _selectedLanguage;
    private SetupLanguageResources _ui;
    private string _productName = "Generic Setup";
    private string _publisher = string.Empty;
    private string _welcomeText;
    private string _statusMessage;
    private string _statusMessageSource = "Ready.";
    private string _completionMessage = string.Empty;
    private string _completionMessageSource = string.Empty;
    private string _licenseText = string.Empty;
    private string _installDirectory = string.Empty;
    private string _installedVersion;
    private string _availableUpdateVersion;
    private string _availableUpdateNotes = string.Empty;
    private string _supportLinkText;
    private string _updateLinkText;
    private string? _supportUrl;
    private string? _updateUrl;
    private bool _acceptLicense = true;
    private bool _createShortcuts = true;
    private bool _enableAutoStart;
    private bool _installForAllUsers;
    private bool _allowMachineInstall = true;
    private bool _purgeData;
    private bool _isBusy;
    private bool _isInstalled;
    private bool _operationSucceeded;
    private double _progressValue;
    private Brush _accentBrush = Brushes.Teal;
    private UpdateAvailabilityState _updateAvailabilityState = UpdateAvailabilityState.NotChecked;
    private string? _resolvedAvailableUpdateVersion;

    public MainWindowViewModel(SetupServices services, SetupEngine engine, RuntimeOptions? startupOptions = null)
    {
        _services = services;
        _engine = engine;
        _startupOptions = startupOptions;

        _selectedLanguage = SetupLanguageCatalog.ResolveDefaultLanguage();
        _ui = SetupLanguageCatalog.Get(_selectedLanguage);
        _welcomeText = string.Format(_ui.DefaultWelcomeTemplate, _productName);
        _statusMessage = SetupStatusTranslator.Translate(_statusMessageSource, _selectedLanguage, _ui);
        _installedVersion = _ui.NotInstalledStatusText;
        _availableUpdateVersion = _ui.NotCheckedStatusText;
        _supportLinkText = _ui.SupportLinkFallbackText;
        _updateLinkText = _ui.UpdateLinkFallbackText;
    }

    public WizardPageKind CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public ImageSource? WindowIcon
    {
        get => _windowIcon;
        private set => SetProperty(ref _windowIcon, value);
    }

    public UiLanguage SelectedLanguage
    {
        get => _selectedLanguage;
        private set
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
        set
        {
            if (value)
            {
                SelectedLanguage = UiLanguage.English;
            }
        }
    }

    public bool IsChineseSelected
    {
        get => SelectedLanguage == UiLanguage.ChineseSimplified;
        set
        {
            if (value)
            {
                SelectedLanguage = UiLanguage.ChineseSimplified;
            }
        }
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

    public string LicenseText
    {
        get => _licenseText;
        private set
        {
            if (SetProperty(ref _licenseText, value))
            {
                RaisePropertyChanged(nameof(HasLicense));
                RaisePropertyChanged(nameof(CanExecuteAction));
            }
        }
    }

    public string InstallDirectory
    {
        get => _installDirectory;
        set => SetProperty(ref _installDirectory, value);
    }

    public string InstalledVersion
    {
        get => _installedVersion;
        private set
        {
            if (SetProperty(ref _installedVersion, value))
            {
                RaisePropertyChanged(nameof(MaintenanceInstalledVersionText));
            }
        }
    }

    public string AvailableUpdateVersion
    {
        get => _availableUpdateVersion;
        private set
        {
            if (SetProperty(ref _availableUpdateVersion, value))
            {
                RaisePropertyChanged(nameof(RuntimeUpdateStatusText));
                RaisePropertyChanged(nameof(UpdateAvailableVersionText));
            }
        }
    }

    public string AvailableUpdateNotes
    {
        get => _availableUpdateNotes;
        private set => SetProperty(ref _availableUpdateNotes, value);
    }

    public string SupportLinkText
    {
        get => _supportLinkText;
        private set => SetProperty(ref _supportLinkText, value);
    }

    public string UpdateLinkText
    {
        get => _updateLinkText;
        private set => SetProperty(ref _updateLinkText, value);
    }

    public bool AcceptLicense
    {
        get => _acceptLicense;
        set
        {
            if (SetProperty(ref _acceptLicense, value))
            {
                RaisePropertyChanged(nameof(CanExecuteAction));
            }
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

    public bool InstallForAllUsers
    {
        get => _installForAllUsers;
        set
        {
            if (!AllowMachineInstall && value)
            {
                return;
            }

            if (SetProperty(ref _installForAllUsers, value))
            {
                if (_product != null && _installedState == null)
                {
                    InstallDirectory = _services.Paths.GetDefaultInstallDirectory(_product, GetSelectedScope());
                }
            }
        }
    }

    public bool AllowMachineInstall
    {
        get => _allowMachineInstall;
        private set => SetProperty(ref _allowMachineInstall, value);
    }

    public bool PurgeData
    {
        get => _purgeData;
        set => SetProperty(ref _purgeData, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RaisePropertyChanged(nameof(CanExecuteAction));
                RaisePropertyChanged(nameof(CanChangeInstallDirectory));
                RaisePropertyChanged(nameof(InstallDirectoryHintText));
            }
        }
    }

    public bool IsInstalled
    {
        get => _isInstalled;
        private set
        {
            if (SetProperty(ref _isInstalled, value))
            {
                RaisePropertyChanged(nameof(CanLaunchInstalledApplication));
                RaisePropertyChanged(nameof(CanChangeInstallDirectory));
                RaisePropertyChanged(nameof(InstallDirectoryHintText));
                RaisePropertyChanged(nameof(InstallPrimaryActionText));
            }
        }
    }

    public bool OperationSucceeded
    {
        get => _operationSucceeded;
        private set => SetProperty(ref _operationSucceeded, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        private set => SetProperty(ref _progressValue, value);
    }

    public Brush AccentBrush
    {
        get => _accentBrush;
        private set => SetProperty(ref _accentBrush, value);
    }

    public bool HasLicense => !string.IsNullOrWhiteSpace(LicenseText);

    public bool HasSupportLink => !string.IsNullOrWhiteSpace(_supportUrl);

    public bool HasUpdateLink => !string.IsNullOrWhiteSpace(_updateUrl);

    public bool CanLaunchInstalledApplication => IsInstalled && _installedState != null && File.Exists(_installedState.MainExecutablePath);

    public bool CanExecuteAction => !IsBusy && (!HasLicense || AcceptLicense);

    public bool ShouldAutoRunStartupOperation => _startupOptions != null;

    public bool CanChangeInstallDirectory => !IsBusy && _installedState == null;

    public string InstallDirectoryHintText => CanChangeInstallDirectory ? Ui.InstallDirectoryEditableHint : Ui.InstallDirectoryLockedHint;

    public string InstallPrimaryActionText => IsInstalled ? Ui.ApplyChangesButtonText : Ui.InstallButtonText;

    public string RuntimeUpdateStatusText => Ui.FormatUpdateStatus(AvailableUpdateVersion);

    public string MaintenanceInstalledVersionText => Ui.FormatInstalledVersion(InstalledVersion);

    public string UpdateAvailableVersionText => Ui.FormatAvailableVersion(AvailableUpdateVersion);

    public string ErrorDialogTitle => Ui.ErrorDialogTitle;

    public string SelectFolderDialogDescription => Ui.SelectFolderDialogDescription;

    public Task InitializeAsync()
    {
        _productManifestPath = ResolveProductManifestPath();
        ProductManifestLoadResult loadResult = ProductManifestLoader.Load(_productManifestPath, _services.Serializer);
        if (loadResult.Errors.Count > 0)
        {
            OperationSucceeded = false;
            string error = string.Join(Environment.NewLine, loadResult.Errors);
            SetCompletionMessageSource(error);
            SetStatusMessageSource(error);
            CurrentPage = WizardPageKind.Complete;
            return Task.CompletedTask;
        }

        _product = loadResult.Manifest ?? throw new InvalidOperationException("Product manifest could not be loaded.");
        Publisher = _product.Publisher;
        _supportUrl = _product.SupportUrl;
        _updateUrl = _product.UpdateInfoUrl;
        RaisePropertyChanged(nameof(HasSupportLink));
        RaisePropertyChanged(nameof(HasUpdateLink));
        AccentBrush = ParseBrush(_product.Branding.AccentColor);
        WindowIcon = LoadWindowIcon(_product, _productManifestPath);
        LicenseText = LoadLicenseText(_product, _productManifestPath);
        AcceptLicense = !HasLicense;

        _installedState = InstalledStateLocator.TryLoad(_product, null, _services.Paths, _services.Serializer, _services.FileSystem);
        AllowMachineInstall = _product.InstallDefaults.AllowMachineInstall;
        InstallForAllUsers = _installedState?.InstallScope == InstallScope.AllUsers ||
            (_installedState == null && _product.InstallDefaults.DefaultScope == InstallScope.AllUsers && AllowMachineInstall);
        IsInstalled = _installedState != null;
        RefreshInstalledVersion();
        InstallDirectory = _installedState?.InstallDirectory ?? _services.Paths.GetDefaultInstallDirectory(_product, GetSelectedScope());
        CreateShortcuts = _product.InstallDefaults.CreateShortcutsByDefault;
        EnableAutoStart = _installedState?.AutorunEnabled ?? _product.InstallDefaults.EnableAutoStartByDefault;
        PurgeData = _product.Uninstall.PurgeDataByDefault;
        _updateAvailabilityState = UpdateAvailabilityState.NotChecked;
        _resolvedAvailableUpdateVersion = null;
        AvailableUpdateNotes = string.Empty;
        RefreshAvailableUpdateVersion();
        ApplyLanguage();

        if (_startupOptions == null)
        {
            CurrentPage = IsInstalled ? WizardPageKind.Maintenance : WizardPageKind.Welcome;
        }

        return Task.CompletedTask;
    }

    public async Task RunStartupOperationAsync()
    {
        if (_startupOptions == null)
        {
            return;
        }

        await ExecuteAsync(_startupOptions).ConfigureAwait(true);
    }

    public void ShowWelcome() => CurrentPage = WizardPageKind.Welcome;

    public void ShowLicenseOrInstall()
    {
        CurrentPage = HasLicense ? WizardPageKind.License : WizardPageKind.InstallOptions;
    }

    public void ShowInstallOptions() => CurrentPage = WizardPageKind.InstallOptions;

    public void ShowMaintenance() => CurrentPage = WizardPageKind.Maintenance;

    public void ShowUninstallConfirmation() => CurrentPage = WizardPageKind.UninstallConfirm;

    public async Task ShowUpdateAsync()
    {
        await CheckForUpdatesAsync(moveToUpdatePage: true).ConfigureAwait(true);
    }

    public void SetInstallDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        InstallDirectory = Path.GetFullPath(path);
    }

    public void ResetInstallDirectory()
    {
        if (_product == null)
        {
            return;
        }

        InstallDirectory = _installedState?.InstallDirectory ?? _services.Paths.GetDefaultInstallDirectory(_product, GetSelectedScope());
    }

    public RuntimeOptions CreateOptions(SetupMode mode)
    {
        if (_product == null)
        {
            throw new InvalidOperationException("Product manifest has not been loaded.");
        }

        return new RuntimeOptions
        {
            Mode = mode,
            Scope = GetSelectedScope(),
            ProductManifestPath = _productManifestPath,
            InstallDirectory = InstallDirectory,
            NoShortcuts = !CreateShortcuts,
            NoAutostart = !EnableAutoStart,
            PurgeData = PurgeData,
            SkipLaunch = true
        };
    }

    public string[] BuildArguments(RuntimeOptions options)
    {
        List<string> arguments =
        [
            "--mode",
            options.Mode.ToString().ToLowerInvariant(),
            "--scope",
            options.Scope == InstallScope.AllUsers ? "machine" : "user",
            "--product",
            _productManifestPath,
            "--skip-launch"
        ];

        if (!string.IsNullOrWhiteSpace(options.InstallDirectory))
        {
            arguments.Add("--install-dir");
            arguments.Add(options.InstallDirectory);
        }

        if (options.NoShortcuts)
        {
            arguments.Add("--no-shortcuts");
        }

        if (options.NoAutostart)
        {
            arguments.Add("--no-autostart");
        }

        if (options.PurgeData)
        {
            arguments.Add("--purge-data");
        }

        if (options.Elevated)
        {
            arguments.Add("--elevated");
        }

        if (options.LaunchAfterInstall)
        {
            arguments.Add("--launch");
        }

        return arguments.ToArray();
    }

    public async Task ExecuteAsync(RuntimeOptions options)
    {
        IsBusy = true;
        OperationSucceeded = false;
        CurrentPage = WizardPageKind.Progress;
        ProgressValue = 0;
        SetStatusMessageSource(CreateRunningOperationSource(options.Mode));

        Progress<SetupProgress> progress = new(step =>
        {
            ProgressValue = step.Percent;
            SetStatusMessageSource(step.Message);
        });

        try
        {
            SetupOperationResult result = await _engine.ExecuteAsync(options, progress, CancellationToken.None).ConfigureAwait(true);
            OperationSucceeded = result.Succeeded;
            SetStatusMessageSource(result.Message);
            SetCompletionMessageSource(result.Message);
            ProgressValue = result.Succeeded ? 100 : ProgressValue;
            _installedState = result.InstalledState ?? (_product != null
                ? InstalledStateLocator.TryLoad(_product, null, _services.Paths, _services.Serializer, _services.FileSystem)
                : null);
            IsInstalled = _installedState != null;
            RefreshInstalledVersion();
            if (_installedState != null)
            {
                InstallDirectory = _installedState.InstallDirectory;
                EnableAutoStart = _installedState.AutorunEnabled;
            }
            else if (_product != null)
            {
                InstallDirectory = _services.Paths.GetDefaultInstallDirectory(_product, GetSelectedScope());
            }

            CurrentPage = WizardPageKind.Complete;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CheckForUpdatesAsync(bool moveToUpdatePage = false)
    {
        if (_product == null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            UpdateFeedManifest? update = await _engine.CheckForUpdatesAsync(_productManifestPath, CancellationToken.None).ConfigureAwait(true);
            _updateAvailabilityState = update == null ? UpdateAvailabilityState.NoUpdates : UpdateAvailabilityState.Available;
            _resolvedAvailableUpdateVersion = update?.Version;
            AvailableUpdateNotes = update?.ReleaseNotes ?? string.Empty;
            RefreshAvailableUpdateVersion();
            SetStatusMessageSource(update == null
                ? "No update is available."
                : $"Update {update.Version} is available.");
            if (moveToUpdatePage)
            {
                CurrentPage = WizardPageKind.Update;
            }
        }
        catch (Exception ex)
        {
            _updateAvailabilityState = UpdateAvailabilityState.Failed;
            _resolvedAvailableUpdateVersion = null;
            AvailableUpdateNotes = ex.Message;
            RefreshAvailableUpdateVersion();
            SetStatusMessageSource("Check failed");
            if (moveToUpdatePage)
            {
                CurrentPage = WizardPageKind.Update;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void LaunchInstalledApplication()
    {
        if (_installedState == null || !File.Exists(_installedState.MainExecutablePath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _installedState.MainExecutablePath,
            WorkingDirectory = _installedState.InstallDirectory,
            UseShellExecute = true
        });
    }

    public void OpenSupportLink() => OpenUrl(_supportUrl);

    public void OpenUpdateLink() => OpenUrl(_updateUrl);

    private void ApplyLanguage()
    {
        Ui = SetupLanguageCatalog.Get(SelectedLanguage);
        RaisePropertyChanged(nameof(IsEnglishSelected));
        RaisePropertyChanged(nameof(IsChineseSelected));
        RefreshBrandingText();
        RefreshInstalledVersion();
        RefreshAvailableUpdateVersion();
        SetStatusMessageSource(_statusMessageSource);
        SetCompletionMessageSource(_completionMessageSource);
        RaisePropertyChanged(nameof(InstallDirectoryHintText));
        RaisePropertyChanged(nameof(InstallPrimaryActionText));
        RaisePropertyChanged(nameof(RuntimeUpdateStatusText));
        RaisePropertyChanged(nameof(MaintenanceInstalledVersionText));
        RaisePropertyChanged(nameof(UpdateAvailableVersionText));
        RaisePropertyChanged(nameof(ErrorDialogTitle));
        RaisePropertyChanged(nameof(SelectFolderDialogDescription));
    }

    private void RefreshBrandingText()
    {
        if (_product == null)
        {
            ProductName = "Generic Setup";
            WelcomeText = string.Format(Ui.DefaultWelcomeTemplate, ProductName);
            SupportLinkText = Ui.SupportLinkFallbackText;
            UpdateLinkText = Ui.UpdateLinkFallbackText;
            return;
        }

        ProductName = LocalizedManifestTextResolver.Resolve(_product.DisplayNameLocalized, _product.DisplayName, SelectedLanguage);

        string defaultWelcomeText = string.IsNullOrWhiteSpace(_product.Branding.WelcomeText)
            ? string.Format(Ui.DefaultWelcomeTemplate, ProductName)
            : _product.Branding.WelcomeText!;
        WelcomeText = LocalizedManifestTextResolver.Resolve(_product.Branding.WelcomeTextLocalized, defaultWelcomeText, SelectedLanguage);

        string supportFallback = string.IsNullOrWhiteSpace(_product.Branding.SupportLinkText)
            ? Ui.SupportLinkFallbackText
            : _product.Branding.SupportLinkText;
        SupportLinkText = LocalizedManifestTextResolver.Resolve(_product.Branding.SupportLinkTextLocalized, supportFallback, SelectedLanguage);

        string updateFallback = string.IsNullOrWhiteSpace(_product.Branding.UpdateLinkText)
            ? Ui.UpdateLinkFallbackText
            : _product.Branding.UpdateLinkText;
        UpdateLinkText = LocalizedManifestTextResolver.Resolve(_product.Branding.UpdateLinkTextLocalized, updateFallback, SelectedLanguage);
    }

    private void RefreshInstalledVersion()
    {
        InstalledVersion = _installedState?.Version ?? Ui.NotInstalledStatusText;
    }

    private void RefreshAvailableUpdateVersion()
    {
        AvailableUpdateVersion = _updateAvailabilityState switch
        {
            UpdateAvailabilityState.Available => _resolvedAvailableUpdateVersion ?? Ui.NotCheckedStatusText,
            UpdateAvailabilityState.NoUpdates => Ui.NoUpdatesStatusText,
            UpdateAvailabilityState.Failed => Ui.CheckFailedStatusText,
            _ => Ui.NotCheckedStatusText
        };
    }

    private void SetStatusMessageSource(string value)
    {
        _statusMessageSource = value;
        StatusMessage = SetupStatusTranslator.Translate(value, SelectedLanguage, Ui);
    }

    private void SetCompletionMessageSource(string value)
    {
        _completionMessageSource = value;
        CompletionMessage = SetupStatusTranslator.Translate(value, SelectedLanguage, Ui);
    }

    private InstallScope GetSelectedScope()
    {
        return InstallForAllUsers ? InstallScope.AllUsers : InstallScope.CurrentUser;
    }

    private string ResolveProductManifestPath()
    {
        string payloadManifest = Path.Combine(_services.Paths.GetPayloadDirectory(), SetupRuntimeDefaults.ProductManifestFileName);
        if (_services.FileSystem.FileExists(payloadManifest))
        {
            return payloadManifest;
        }

        string directManifest = Path.Combine(_services.Paths.AppBaseDirectory, SetupRuntimeDefaults.ProductManifestFileName);
        if (_services.FileSystem.FileExists(directManifest))
        {
            return directManifest;
        }

        throw new FileNotFoundException("Unable to locate product.json in the installer payload.");
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
        if (!File.Exists(iconPath))
        {
            return null;
        }

        try
        {
            return BitmapFrame.Create(new Uri(iconPath, UriKind.Absolute));
        }
        catch
        {
            return null;
        }
    }

    private static Brush ParseBrush(string color)
    {
        try
        {
            return (Brush)new BrushConverter().ConvertFromString(color)!;
        }
        catch
        {
            return Brushes.Teal;
        }
    }

    private static string CreateRunningOperationSource(SetupMode mode)
    {
        return mode switch
        {
            SetupMode.Repair => "Running repair...",
            SetupMode.Update => "Running update...",
            SetupMode.Uninstall => "Running uninstall...",
            _ => "Running install..."
        };
    }

    private static void OpenUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
