using RS.Widgets.Enums;
using RS.Widgets.Models;
using RS.Win32API;
using RS.Win32API.Structs;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using ZXing.OneD;

namespace RS.Widgets.Controls
{
    /// <summary>
    /// RSNavPopup.xaml 的交互逻辑
    /// </summary>
    public partial class RSNavPopup : Window
    {
        public RSNavItem RSListBoxItem { get; set; }
        public RSNavigate RSNavigate { get; set; }
        public RSNavPopup RSNavPopupParent { get; set; }

     
     
        public RSNavPopup(RSNavigate rSNavigate, RSNavPopup rsNavPopupParent, RSNavItem rsListBoxItem, List<NavigateModel> navigateModelList)
        {
            InitializeComponent();
            this.Owner = rSNavigate.ParentWin;
            this.RSNavigate = rSNavigate;
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.ShowInTaskbar = false;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.RSListBoxItem = rsListBoxItem;
            this.RSNavPopupParent = rsNavPopupParent;
            this.NavigateModelList = new ObservableCollection<NavigateModel>(navigateModelList);

            this.Owner.PreviewMouseDown += Owner_PreviewMouseDown;
            this.Owner.SizeChanged += Owner_SizeChanged;
            this.Owner.LocationChanged += Owner_LocationChanged;

            this.Loaded += RSNavPopup_Loaded;

            this.Unloaded += RSNavPopup_Unloaded;

            this.PreviewKeyDown += RSNavPopup_PreviewKeyDown;

            this.PreviewMouseRightButtonDown += RSNavPopup_PreviewMouseRightButtonDown;
        }

        private void RSNavPopup_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.CascadingDestroy(CascadeDeleteDirection.Both);
        }

        private void RSNavPopup_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.CascadingDestroy(CascadeDeleteDirection.Both);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            this.UpdatePosition();

            var workingArea = this.GetCurrentScreenWorkingArea();
            var left = this.Left;
            var top = this.Top;
            var actualWidth = this.ActualWidth;
            var actualHeight = this.ActualHeight;

            if (top + this.Height > workingArea.Height)
            {
                var topShould = workingArea.Height - this.Height;
                this.Top = Math.Max(10, topShould);
                if (topShould < 10)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.SizeToContent = SizeToContent.Width;
                        this.Height = workingArea.Height - 20;
                    }, DispatcherPriority.Input);
                }
            }

        }
        private void Owner_LocationChanged(object? sender, EventArgs e)
        {
            this.DestroyWin();
        }

        private void Owner_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.DestroyWin();
        }

        private void RSNavPopup_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void RSNavPopup_Unloaded(object sender, RoutedEventArgs e)
        {
            UnbindEvents();
        }
        private void UnbindEvents()
        {
            this.Owner.PreviewMouseLeftButtonDown -= Owner_PreviewMouseDown;
            this.Owner.SizeChanged -= Owner_SizeChanged;
            this.Owner.LocationChanged -= Owner_LocationChanged;
            this.PreviewKeyDown -= RSNavPopup_PreviewKeyDown;
            this.PreviewMouseRightButtonDown -= RSNavPopup_PreviewMouseRightButtonDown;
        }

        private void Owner_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DestroyWin();
        }

        public void DestroyWin()
        {
            this.Close();
        }

        public void CascadingDestroy(CascadeDeleteDirection cascadeDeleteDirection)
        {

            RSNavPopup rsNavPopup = this.FindChild<RSNavList>()?.RSNavPopup;
            switch (cascadeDeleteDirection)
            {
                case CascadeDeleteDirection.Up:
                    this.RSNavPopupParent?.CascadingDestroy(cascadeDeleteDirection);
                    break;
                case CascadeDeleteDirection.Down:
                    rsNavPopup?.CascadingDestroy(cascadeDeleteDirection);
                    break;
                case CascadeDeleteDirection.Both:
                    rsNavPopup?.CascadingDestroy(CascadeDeleteDirection.Down);
                    this.RSNavPopupParent?.CascadingDestroy(CascadeDeleteDirection.Up);
                    break;
            }

            this.DestroyWin();
        }



        public ObservableCollection<NavigateModel> NavigateModelList
        {
            get { return (ObservableCollection<NavigateModel>)GetValue(NavigateModelListProperty); }
            set { SetValue(NavigateModelListProperty, value); }
        }
        public static readonly DependencyProperty NavigateModelListProperty =
            DependencyProperty.Register("NavigateModelList", typeof(ObservableCollection<NavigateModel>), typeof(RSNavPopup), new PropertyMetadata(null));


        public void UpdatePosition()
        {
            //显示在导航的右边
            var rsListBoxItemPosition = this.RSListBoxItem.PointToScreen(new Point(0, 0));
            this.Left = rsListBoxItemPosition.X + this.RSListBoxItem.ActualWidth - 6;
            this.Top = rsListBoxItemPosition.Y - 13;
        }

        public Rect GetCurrentScreenWorkingArea()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            IntPtr hMonitor = NativeMethods.MonitorFromWindow(hwnd, 2u);
            MONITORINFO monitorInfo = NativeMethods.GetMonitorInfo(hMonitor);
            RECT rcWork = monitorInfo.rcWork;
            var rc = monitorInfo.rcWork;
            return new Rect(rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top);
        }

        internal void ExpandParentNav()
        {
            var navigateModel = this.RSListBoxItem.DataContext as NavigateModel;
            if (navigateModel != null)
            {
                navigateModel.IsExpand = true;
            }
            this.RSNavPopupParent?.ExpandParentNav();
        }
    }
}
