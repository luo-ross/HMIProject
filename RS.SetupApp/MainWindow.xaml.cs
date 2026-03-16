using System.IO;
using System.Windows;
using RS.SetupApp.Core;
using RS.SetupApp.ViewModels;

namespace RS.SetupApp;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.InitializeAsync().ConfigureAwait(true);
            if (_viewModel.ShouldAutoRunStartupOperation)
            {
                await _viewModel.RunStartupOperationAsync().ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, _viewModel.ErrorDialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void WelcomeContinue_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ShowLicenseOrInstall();
    }

    private void LicenseBack_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ShowWelcome();
    }

    private void LicenseContinue_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.CanExecuteAction)
        {
            _viewModel.ShowInstallOptions();
        }
    }

    private void InstallBack_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsInstalled)
        {
            _viewModel.ShowMaintenance();
            return;
        }

        _viewModel.ShowWelcome();
    }

    private async void InstallStart_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteAsync(SetupMode.Install).ConfigureAwait(true);
    }

    private async void MaintenanceRepair_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteAsync(SetupMode.Repair).ConfigureAwait(true);
    }

    private async void MaintenanceCheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.CheckForUpdatesAsync().ConfigureAwait(true);
    }

    private async void MaintenanceUpdate_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ShowUpdateAsync().ConfigureAwait(true);
    }

    private void MaintenanceReinstall_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ShowInstallOptions();
    }

    private void MaintenanceUninstall_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ShowUninstallConfirmation();
    }

    private void UpdateBack_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ShowMaintenance();
    }

    private async void UpdateStart_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteAsync(SetupMode.Update).ConfigureAwait(true);
    }

    private void UninstallBack_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ShowMaintenance();
    }

    private async void UninstallStart_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteAsync(SetupMode.Uninstall).ConfigureAwait(true);
    }

    private void CompleteClose_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void LaunchInstalledApp_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.LaunchInstalledApplication();
    }

    private void Support_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenSupportLink();
    }

    private void ReleaseNotes_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenUpdateLink();
    }

    private void BrowseInstallDirectory_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFolderDialog dialog = new()
        {
            Title = _viewModel.SelectFolderDialogDescription,
            Multiselect = false
        };

        if (!string.IsNullOrWhiteSpace(_viewModel.InstallDirectory) && Directory.Exists(_viewModel.InstallDirectory))
        {
            dialog.InitialDirectory = _viewModel.InstallDirectory;
            dialog.FolderName = _viewModel.InstallDirectory;
        }

        if (dialog.ShowDialog(this) == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
        {
            _viewModel.SetInstallDirectory(dialog.FolderName);
        }
    }

    private void ResetInstallDirectory_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ResetInstallDirectory();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        {
            DragMove();
        }
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async Task ExecuteAsync(SetupMode mode)
    {
        if (!_viewModel.CanExecuteAction)
        {
            return;
        }

        RuntimeOptions options = _viewModel.CreateOptions(mode);
        string[] args = _viewModel.BuildArguments(options);

        if (await ElevationLauncher.TryRelaunchElevatedAsync(options, args, CancellationToken.None).ConfigureAwait(true))
        {
            System.Windows.Application.Current.Shutdown();
            return;
        }

        if (await SelfWorkerLauncher.TryRelaunchAsync(options, args, CancellationToken.None).ConfigureAwait(true))
        {
            System.Windows.Application.Current.Shutdown();
            return;
        }

        await _viewModel.ExecuteAsync(options).ConfigureAwait(true);
    }
}
