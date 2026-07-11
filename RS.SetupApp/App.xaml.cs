using RS.SetupApp.Core;
using RS.SetupApp.Services;
using RS.SetupApp.ViewModels;

namespace RS.SetupApp;

public partial class App : System.Windows.Application
{
    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
        base.OnStartup(e);

        SetupServices services = SetupServicesFactory.Create();
        SetupEngine engine = new(services);
        string[] args = e.Args;

        if (args.Length > 0)
        {
            RuntimeOptions options = RuntimeArgumentParser.Parse(args);

            try
            {
                if (await ElevationLauncher.TryRelaunchElevatedAsync(options, args, CancellationToken.None).ConfigureAwait(true))
                {
                    Shutdown();
                    return;
                }

                if (await SelfWorkerLauncher.TryRelaunchAsync(options, args, CancellationToken.None).ConfigureAwait(true))
                {
                    Shutdown();
                    return;
                }

                if (options.Silent)
                {
                    SetupOperationResult result = await engine.ExecuteAsync(options, cancellationToken: CancellationToken.None).ConfigureAwait(true);
                    ExitSilent(RuntimeArgumentParser.GetSilentExitCode(result));
                    return;
                }

                ShowMainWindow(services, engine, options);
                return;
            }
            catch (Exception ex)
            {
                if (options.Silent)
                {
                    ExitSilent(3);
                    return;
                }

                System.Windows.MessageBox.Show(ex.Message, "Setup", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                Shutdown();
                return;
            }
        }

        ShowMainWindow(services, engine, startupOptions: null);
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        SelfWorkerLauncher.TryCleanupCurrentWorkerDirectory();
        base.OnExit(e);
    }

    private void ExitSilent(int exitCode)
    {
        Shutdown(exitCode);
    }

    private void ShowMainWindow(SetupServices services, SetupEngine engine, RuntimeOptions? startupOptions)
    {
        MainWindowViewModel viewModel = new(
            new SetupWorkflow(services, engine),
            new SetupRelaunchService(),
            new FolderPicker(),
            new ExternalLauncher(),
            new SetupDialogService(),
            startupOptions);
        MainWindow window = new(viewModel);
        MainWindow = window;
        ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
        window.Show();
    }
}
