using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using RS.Win32API;
using RS.Win32API.Structs;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace RS.Widgets.Controls
{

    public class RSDockGrid : Grid
    {
        public static HashSet<RSDockGrid> RSDockGridList { get; private set; }

        /// <summary>
        /// 记录Panel在屏幕上的位置
        /// </summary>
        public Rect ScreenRect { get; private set; }
        public Window? ParentWindow { get; private set; }
        public IntPtr ParentWindowHandle { get; private set; }

        static RSDockGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDockGrid), new FrameworkPropertyMetadata(typeof(RSDockGrid)));
            RSDockGridList = new HashSet<RSDockGrid>();
        }

        public RSDockGrid()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Margin = new Thickness(5);
            this.Loaded += RSDockGrid_Loaded;
            this.SizeChanged += RSDockGrid_SizeChanged;
            this.Unloaded += RSDockGrid_Unloaded;
            AddRRSDockGrid(this);
        }


        public static void RemoveRSDockGrid(RSDockGrid dockGrid)
        {
            if (RSDockGridList.Contains(dockGrid))
            {
                RSDockGridList.Remove(dockGrid);
            }
        }

        public static void AddRRSDockGrid(RSDockGrid dockGrid)
        {
            RSDockGridList.Add(dockGrid);
        }

        public static RSDockGrid? GetRSDockGrid(POINT cursorPos)
        {
            return GetRSDockGridList(cursorPos).FirstOrDefault();
        }

        public static List<RSDockGrid> GetRSDockGridListUnderCursorPos()
        {
            var cursorPos = NativeMethods.GetCursorPos();
            return GetRSDockGridList(cursorPos);
        }

        public static List<RSDockGrid> GetRSDockGridList(POINT cursorPos)
        {

            return RSDockGridList.Where(t => t.ScreenRect.X <= cursorPos.X
            && t.ScreenRect.Y <= cursorPos.Y
            && t.ScreenRect.Right >= cursorPos.X
            && t.ScreenRect.Bottom >= cursorPos.Y).ToList();
        }



        private void RSDockGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            this.UnRegisterWindowEvents(this.ParentWindow);
        }

        private void UnRegisterWindowEvents(Window? window)
        {
            if (window == null)
            {
                return;
            }
            window.LocationChanged -= ParentWindow_LocationChanged;
            window.Closed -= Window_Closed;
        }

        private void RSDockGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RefreshScreenRect();
        }

        private void RSDockGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //获取父窗体
            this.ParentWindow = this.TryFindParent<Window>();
            if (this.ParentWindow == null)
            {
                this.ParentWindowHandle = new WindowInteropHelper(this.ParentWindow).Handle;
            }
            this.RegisterWindowEvents(this.ParentWindow);
            this.RefreshScreenRect();
        }

        private void RefreshScreenRect()
        {
            if (PresentationSource.FromVisual(this) == null)
            {
                return;
            }

            var screenPoint = this.PointToScreen(new Point(0, 0));
            this.ScreenRect = new Rect(screenPoint.X,
                                                 screenPoint.Y,
                                                 this.ActualWidth,
                                                 this.ActualHeight);
        }

        private void RegisterWindowEvents(Window window)
        {
            if (window == null)
            {
                return;
            }
            window.LocationChanged -= ParentWindow_LocationChanged;
            window.Closed -= Window_Closed;
            window.LocationChanged += ParentWindow_LocationChanged;
            window.Closed += Window_Closed;
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            RemoveRSDockGrid(this);
        }

        private void ParentWindow_LocationChanged(object? sender, EventArgs e)
        {
            this.RefreshScreenRect();
        }

        public void AddDockPanel(PositionEnum positionEnum, FrameworkElement frameworkElement)
        {
            double minColumnWidth = 35D;
            double minRowHeight = 35D;
            double splitGridLength = 5;
            RSDockPanel dockDragPanel = null;
            switch (positionEnum)
            {
                case PositionEnum.UnKnown:
                    break;
                case PositionEnum.WindowLeft:
                    dockDragPanel = AddWindowLeftDockPanel(frameworkElement, minColumnWidth, splitGridLength);
                    break;
                case PositionEnum.WindowTop:
                    dockDragPanel = AddWindowTopDockPanel(frameworkElement, minRowHeight, splitGridLength);
                    break;
                case PositionEnum.WindowRight:
                    dockDragPanel = AddWindowRightDockPanel(frameworkElement, minColumnWidth, splitGridLength);
                    break;
                case PositionEnum.WindowBottom:
                    dockDragPanel = AddWindowBottomDockPanel(frameworkElement, minRowHeight, splitGridLength);
                    break;
                case PositionEnum.TabLeft:
                    break;
                case PositionEnum.TabTop:
                    break;
                case PositionEnum.TabRight:
                    break;
                case PositionEnum.TabBottom:
                    break;
                case PositionEnum.TabCenter:
                    break;
            }

            if (dockDragPanel == null)
            {
                return;
            }


        }

        private RSDockPanel AddWindowLeftDockPanel(FrameworkElement frameworkElement, double minColumnWidth, double splitGridLength)
        {
            if (this.ColumnDefinitions.Count == 0)
            {
                this.ColumnDefinitions.Insert(0, new ColumnDefinition()
                {
                    MinWidth = minColumnWidth,
                });
            }

            this.ColumnDefinitions.Insert(0, new ColumnDefinition()
            {
                Width = new GridLength(splitGridLength),
            });

            this.ColumnDefinitions.Insert(0, new ColumnDefinition()
            {
                MinWidth = minColumnWidth,
            });

            foreach (FrameworkElement item in this.Children)
            {
                var colum = Grid.GetColumn(item);
                Grid.SetColumn(item, colum + 2);
            }

            var gridSplitter = GetGridSplitter(0, 1, this.RowDefinitions.Count, 1);
            this.Children.Insert(0, gridSplitter);

            var dockDragPanel = GetDockDragPanel(frameworkElement, 0, 0, this.RowDefinitions.Count, 1);
            this.Children.Insert(0, dockDragPanel);

            dockDragPanel.GridSplitter = gridSplitter;
            return dockDragPanel;
        }

        private RSDockPanel AddWindowTopDockPanel(FrameworkElement frameworkElement, double minRowHeight, double splitGridLength)
        {
            if (this.RowDefinitions.Count == 0)
            {
                this.RowDefinitions.Insert(0, new RowDefinition()
                {
                    MinHeight = minRowHeight,
                });
            }

            this.RowDefinitions.Insert(0, new RowDefinition()
            {
                Height = new GridLength(splitGridLength),
            });

            this.RowDefinitions.Insert(0, new RowDefinition()
            {
                MinHeight = minRowHeight,
            });

            foreach (FrameworkElement item in this.Children)
            {
                var row = Grid.GetRow(item);
                Grid.SetRow(item, row + 2);
            }

            var gridSplitter = GetGridSplitter(1, 0, 1, this.ColumnDefinitions.Count);
            this.Children.Insert(0, gridSplitter);

            var dockDragPanel = GetDockDragPanel(frameworkElement, 0, 0, 1, this.ColumnDefinitions.Count);
            this.Children.Insert(0, dockDragPanel);
            dockDragPanel.GridSplitter = gridSplitter;
            return dockDragPanel;
        }

        private RSDockPanel AddWindowRightDockPanel(FrameworkElement frameworkElement, double minColumnWidth, double splitGridLength)
        {

            if (this.ColumnDefinitions.Count == 0)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    MinWidth = minColumnWidth,
                });
            }

            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(splitGridLength),
            });

            var gridSplitter = GetGridSplitter(0, this.ColumnDefinitions.Count - 1, this.RowDefinitions.Count, 1);
            this.Children.Add(gridSplitter);


            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
                MinWidth = minColumnWidth,
            });


            var dockDragPanel = GetDockDragPanel(frameworkElement, 0, this.ColumnDefinitions.Count - 1, this.RowDefinitions.Count, 1);
            this.Children.Add(dockDragPanel);

            dockDragPanel.GridSplitter = gridSplitter;
            return dockDragPanel;
        }

        private RSDockPanel AddWindowBottomDockPanel(FrameworkElement frameworkElement, double minRowHeight, double splitGridLength)
        {
            if (this.RowDefinitions.Count == 0)
            {
                this.RowDefinitions.Add(new RowDefinition()
                {
                    MinHeight = minRowHeight,
                });
            }

            this.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(splitGridLength),
            });

            var gridSplitter = GetGridSplitter(this.RowDefinitions.Count - 1, 0, 1, this.ColumnDefinitions.Count);
            this.Children.Add(gridSplitter);

            this.RowDefinitions.Add(new RowDefinition()
            {
                MinHeight = minRowHeight,
            });

            var dockDragPanel = GetDockDragPanel(frameworkElement, this.RowDefinitions.Count - 1, 0, 1, this.ColumnDefinitions.Count);
            this.Children.Add(dockDragPanel);

            dockDragPanel.GridSplitter = gridSplitter;
            return dockDragPanel;
        }

        private RSDockPanel GetDockDragPanel(FrameworkElement frameworkElement, int row, int column, int rowSpan, int columnSpan)
        {
            rowSpan = rowSpan == 0 ? 1 : rowSpan;
            columnSpan = columnSpan == 0 ? 1 : columnSpan;
            RSDockPanel dockPanel = new RSDockPanel();
            dockPanel.Content = frameworkElement;
            Grid.SetRow(dockPanel, row);
            Grid.SetColumn(dockPanel, column);
            Grid.SetRowSpan(dockPanel, rowSpan);
            Grid.SetColumnSpan(dockPanel, columnSpan);
            return dockPanel;
        }

        private GridSplitter GetGridSplitter(int row, int column, int rowSpan, int columnSpan)
        {
            GridSplitter gridSplitter = new GridSplitter();
            Grid.SetRow(gridSplitter, row);
            Grid.SetColumn(gridSplitter, column);
            Grid.SetRowSpan(gridSplitter, rowSpan == 0 ? 1 : rowSpan);
            Grid.SetColumnSpan(gridSplitter, columnSpan == 0 ? 1 : columnSpan);
            return gridSplitter;
        }


        public void RemoveDockPanel(RSDockPanel dockPanel)
        {
            var gridSplitter= dockPanel.GridSplitter;
            if (gridSplitter != null && this.Children.Contains(gridSplitter))
            {
                this.Children.Remove(gridSplitter);
            }
            if (this.Children.Contains(dockPanel))
            {
                this.Children.Remove(dockPanel);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

    }
}
