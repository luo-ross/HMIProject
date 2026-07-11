using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using RS.SetupApp.ViewModels;

namespace RS.SetupApp;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private bool _isClosingAuthorized;
    private bool _isCloseCoordinatorRunning;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        PreviewKeyDown += MainWindow_PreviewKeyDown;
        _viewModel.RelaunchRequested += ViewModel_RelaunchRequested;
        _viewModel.FinishRequested += ViewModel_FinishRequested;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
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
        catch (Exception exception)
        {
            _viewModel.ReportUnexpectedError(exception);
        }
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_isClosingAuthorized)
        {
            return;
        }

        e.Cancel = true;
        BeginCloseRequest();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            BeginCloseRequest();
        }
    }

    private async void BeginCloseRequest()
    {
        if (_isCloseCoordinatorRunning)
        {
            return;
        }

        _isCloseCoordinatorRunning = true;
        try
        {
            if (await _viewModel.RequestCloseAsync().ConfigureAwait(true))
            {
                _isClosingAuthorized = true;
                Close();
            }
        }
        finally
        {
            _isCloseCoordinatorRunning = false;
        }
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e) => BeginCloseRequest();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void ViewModel_RelaunchRequested(object? sender, EventArgs e)
    {
        _isClosingAuthorized = true;
        Application.Current.Shutdown();
    }

    private void ViewModel_FinishRequested(object? sender, EventArgs e) => BeginCloseRequest();

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.CurrentPage))
        {
            _ = Dispatcher.BeginInvoke(AnimatePageTransition);
        }
    }

    private void AnimatePageTransition()
    {
        if (PageHost.RenderTransform is not TranslateTransform translate)
        {
            return;
        }

        if (!SystemParameters.ClientAreaAnimation)
        {
            PageHost.BeginAnimation(OpacityProperty, null);
            translate.BeginAnimation(TranslateTransform.YProperty, null);
            PageHost.Opacity = 1;
            translate.Y = 0;
            return;
        }

        Duration duration = TimeSpan.FromMilliseconds(160);
        PageHost.Opacity = 0;
        translate.Y = 8;
        PageHost.BeginAnimation(OpacityProperty, new DoubleAnimation(1, duration));
        translate.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, duration));
    }
}
