using NPOI.POIFS.Crypt.Dsig.Services;
using RS.Commons;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RS.Widgets.Controls
{
    public class RSDataGrid : DataGrid
    {
        private DataGrid PART_DataGrid;
        private RSUserControl PART_RSUserControl;
        private Button PART_BtnFirstPage;
        private Button PART_BtnPrevious;
        private Button PART_BtnNext;
        private Button PART_BtnEndPage;
        private CancellationTokenSource LoadDataCTS;


        #region 路由事件

        public static readonly RoutedEvent AddEvent = EventManager.RegisterRoutedEvent(
            "Add", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSDataGrid));
        public static readonly RoutedEvent DeleteEvent = EventManager.RegisterRoutedEvent(
            "Delete", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSDataGrid));
        public static readonly RoutedEvent UpdateEvent = EventManager.RegisterRoutedEvent(
            "Update", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSDataGrid));
        public static readonly RoutedEvent DetailsEvent = EventManager.RegisterRoutedEvent(
            "Details", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSDataGrid));
        public static readonly RoutedEvent ExportEvent = EventManager.RegisterRoutedEvent(
            "Export", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSDataGrid));


        public event RoutedEventHandler Add
        {
            add { AddHandler(AddEvent, value); }
            remove { RemoveHandler(AddEvent, value); }
        }
        public event RoutedEventHandler Delete
        {
            add { AddHandler(DeleteEvent, value); }
            remove { RemoveHandler(DeleteEvent, value); }
        }
        public event RoutedEventHandler Update
        {
            add { AddHandler(UpdateEvent, value); }
            remove { RemoveHandler(UpdateEvent, value); }
        }
        public event RoutedEventHandler Details
        {
            add { AddHandler(DetailsEvent, value); }
            remove { RemoveHandler(DetailsEvent, value); }
        }
        public event RoutedEventHandler Export
        {
            add { AddHandler(ExportEvent, value); }
            remove { RemoveHandler(ExportEvent, value); }
        }
        #endregion

        #region 静态命令

        public static readonly ICommand AddCommand = new RoutedCommand(nameof(AddCommand), typeof(RSDataGrid), new InputGestureCollection
        {
            new KeyGesture(Key.A, ModifierKeys.Alt)
        });
        public static readonly ICommand DeleteCommand = new RoutedCommand(nameof(DeleteCommand), typeof(RSDataGrid), new InputGestureCollection
        {
            new KeyGesture(Key.D, ModifierKeys.Alt)
        });
        public static readonly ICommand UpdateCommand = new RoutedCommand(nameof(UpdateCommand), typeof(RSDataGrid), new InputGestureCollection
        {
            new KeyGesture(Key.U, ModifierKeys.Alt)
        });
        public static readonly ICommand DetailsCommand = new RoutedCommand(nameof(DetailsCommand), typeof(RSDataGrid), new InputGestureCollection
        {
            new KeyGesture(Key.V, ModifierKeys.Alt)
        });
        public static readonly ICommand ExportCommand = new RoutedCommand(nameof(ExportCommand), typeof(RSDataGrid), new InputGestureCollection
        {
            new KeyGesture(Key.E, ModifierKeys.Alt)
        });


        static RSDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDataGrid), new FrameworkPropertyMetadata(typeof(RSDataGrid)));

            CommandManager.RegisterClassCommandBinding(typeof(RSDataGrid),
                new CommandBinding(AddCommand, AddCommandExecuted, CanAddCommandExecute));
            CommandManager.RegisterClassCommandBinding(typeof(RSDataGrid),
                new CommandBinding(DeleteCommand, DeleteCommandExecuted, CanDeleteCommandExecute));
            CommandManager.RegisterClassCommandBinding(typeof(RSDataGrid),
                new CommandBinding(UpdateCommand, UpdateCommandExecuted, CanUpdateCommandExecute));
            CommandManager.RegisterClassCommandBinding(typeof(RSDataGrid),
                new CommandBinding(DetailsCommand, DetailsCommandExecuted, CanDetailsCommandExecute));
            CommandManager.RegisterClassCommandBinding(typeof(RSDataGrid),
                new CommandBinding(ExportCommand, ExportCommandExecuted, CanExportCommandExecute));
        }
        #endregion


        private static void CanExportCommandExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void ExportCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as RSDataGrid;
            if (dataGrid != null)
            {
                dataGrid.RaiseEvent(new RoutedEventArgs(ExportEvent, dataGrid.ItemsSource));
                dataGrid.OnExportCommand?.Execute(dataGrid.ItemsSource);
            }
        }

        private static void CanDetailsCommandExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void DetailsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as RSDataGrid;
            if (dataGrid != null)
            {
                dataGrid.RaiseEvent(new RoutedEventArgs(DetailsEvent, dataGrid.SelectedItem));
                dataGrid.OnDetailsCommand?.Execute(dataGrid.SelectedItem);
            }
        }

        private static void CanUpdateCommandExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void UpdateCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as RSDataGrid;
            if (dataGrid != null)
            {
                dataGrid.RaiseEvent(new RoutedEventArgs(UpdateEvent, dataGrid.SelectedItem));
                dataGrid.OnUpdateCommand?.Execute(dataGrid.SelectedItem);
            }
        }

        private static void CanDeleteCommandExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void DeleteCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as RSDataGrid;
            if (dataGrid != null)
            {
                dataGrid.RaiseEvent(new RoutedEventArgs(DeleteEvent, dataGrid.SelectedItem));
                dataGrid.OnDeleteCommand?.Execute(dataGrid.SelectedItem);
            }
        }

        // 命令执行方法
        private static void AddCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as RSDataGrid;
            if (dataGrid != null)
            {
                dataGrid.RaiseEvent(new RoutedEventArgs(AddEvent, dataGrid.ItemsSource));
                dataGrid.OnAddCommand?.Execute(dataGrid.ItemsSource);
            }
        }

        private static void CanAddCommandExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public RSDataGrid()
        {
            this.Loaded += RSDataGrid_Loaded;
            this.Pagination = new Pagination();
            this.Pagination.OnRowsChanged += Pagination_OnRowsChanged;
            this.MouseEnter += RSDataGrid_MouseEnter;
        }

        private void RSDataGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Focus();
        }

        private async void RSDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = this.PART_DataGrid.FindChild<ScrollViewer>();
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged -= OnScrollChanged;
                scrollViewer.ScrollChanged += OnScrollChanged;
            }
            await LoadData();
        }

        #region 自定义Command
        public static readonly DependencyProperty LoadDataCommandProperty =
        DependencyProperty.Register(nameof(LoadDataCommand), typeof(AsyncRelayCommand<LoadDataArgs, int>), typeof(RSDataGrid), new PropertyMetadata(OnCommandChanged));

        public AsyncRelayCommand<LoadDataArgs, int> LoadDataCommand
        {
            get { return (AsyncRelayCommand<LoadDataArgs, int>)GetValue(LoadDataCommandProperty); }
            set { SetValue(LoadDataCommandProperty, value); }
        }


        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RSDataGrid dataGrid = (RSDataGrid)d;
            dataGrid.OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        private void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
        {
            if (oldCommand != null)
            {
                UnhookCommand(oldCommand);
            }
            if (newCommand != null)
            {
                HookCommand(newCommand);
            }
        }

        private void UnhookCommand(ICommand command)
        {
            CanExecuteChangedEventManager.RemoveHandler(command, OnCanExecuteChanged);
            UpdateCanExecute();
        }

        private void HookCommand(ICommand command)
        {
            CanExecuteChangedEventManager.AddHandler(command, OnCanExecuteChanged);
            UpdateCanExecute();
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateCanExecute();
        }

        private bool UpdateCanExecute()
        {
            if (LoadDataCommand != null)
            {
                return this.LoadDataCommand.CanExecute(null);
            }
            else
            {
                return true;
            }
        }


        #endregion

        #region 只读Command事件

        // 新增数据命令依赖属性
        public ICommand OnAddCommand
        {
            get { return (ICommand)GetValue(OnAddCommandProperty); }
            set { SetValue(OnAddCommandProperty, value); }
        }

        public static readonly DependencyProperty OnAddCommandProperty =
            DependencyProperty.Register("OnAddCommand", typeof(ICommand), typeof(RSDataGrid), new PropertyMetadata(null));



        // 删除选中命令依赖属性

        public ICommand OnDeleteCommand
        {
            get { return (ICommand)GetValue(OnDeleteCommandProperty); }
            set { SetValue(OnDeleteCommandProperty, value); }
        }

        public static readonly DependencyProperty OnDeleteCommandProperty =
            DependencyProperty.Register("OnDeleteCommand", typeof(ICommand), typeof(RSDataGrid), new PropertyMetadata(null));




        // 修改数据命令依赖属性
        public ICommand OnUpdateCommand
        {
            get { return (ICommand)GetValue(OnUpdateCommandProperty); }
            set { SetValue(OnUpdateCommandProperty, value); }
        }

        public static readonly DependencyProperty OnUpdateCommandProperty =
            DependencyProperty.Register("OnUpdateCommand", typeof(ICommand), typeof(RSDataGrid), new PropertyMetadata(null));




        // 查看数据命令依赖属性
        public ICommand OnDetailsCommand
        {
            get { return (ICommand)GetValue(OnDetailsCommandProperty); }
            set { SetValue(OnDetailsCommandProperty, value); }
        }

        public static readonly DependencyProperty OnDetailsCommandProperty =
            DependencyProperty.Register("OnDetailsCommand", typeof(ICommand), typeof(RSDataGrid), new PropertyMetadata(null));




        // 导出配置命令依赖属性
        public ICommand OnExportCommand
        {
            get { return (ICommand)GetValue(OnExportCommandProperty); }
            set { SetValue(OnExportCommandProperty, value); }
        }

        public static readonly DependencyProperty OnExportCommandProperty =
            DependencyProperty.Register("OnExportCommand", typeof(ICommand), typeof(RSDataGrid), new PropertyMetadata(null));

        #endregion


        #region 依赖属性


        public Pagination Pagination
        {
            get { return (Pagination)GetValue(PaginationProperty); }
            set { SetValue(PaginationProperty, value); }
        }

        public static readonly DependencyProperty PaginationProperty =
            DependencyProperty.Register("Pagination", typeof(Pagination), typeof(RSDataGrid), new PropertyMetadata(null));

        #endregion




        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_DataGrid = this.GetTemplateChild(nameof(this.PART_DataGrid)) as DataGrid;
            this.PART_RSUserControl = this.GetTemplateChild(nameof(this.PART_RSUserControl)) as RSUserControl;
            this.PART_BtnFirstPage = this.GetTemplateChild(nameof(this.PART_BtnFirstPage)) as Button;
            this.PART_BtnPrevious = this.GetTemplateChild(nameof(this.PART_BtnPrevious)) as Button;
            this.PART_BtnNext = this.GetTemplateChild(nameof(this.PART_BtnNext)) as Button;
            this.PART_BtnEndPage = this.GetTemplateChild(nameof(this.PART_BtnEndPage)) as Button;

            if (this.PART_DataGrid != null)
            {
                this.PART_DataGrid.Columns.Clear();
                foreach (var column in this.Columns)
                {
                    this.PART_DataGrid.Columns.Add(column);
                }
            }

            if (this.PART_BtnFirstPage != null)
            {
                this.PART_BtnFirstPage.Click += PART_BtnFirstPage_Click;
            }

            if (this.PART_BtnPrevious != null)
            {
                this.PART_BtnPrevious.Click += PART_BtnPrevious_Click;
            }

            if (this.PART_BtnNext != null)
            {
                this.PART_BtnNext.Click += PART_BtnNext_Click;
            }

            if (this.PART_BtnEndPage != null)
            {
                this.PART_BtnEndPage.Click += PART_BtnEndPage_Click;
            }
        }

        private async void PART_BtnEndPage_Click(object sender, RoutedEventArgs e)
        {
            if (this.Pagination.Page == this.Pagination.Total)
            {
                return;
            }
            this.Pagination.Page = this.Pagination.Total;
            await LoadData();
        }

        private async void PART_BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (this.Pagination.Page == this.Pagination.Total)
            {
                return;
            }
            this.Pagination.Page = Math.Min(this.Pagination.Total, this.Pagination.Page + 1);
            await LoadData();
        }

        private async void PART_BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (this.Pagination.Page == 1)
            {
                return;
            }
            this.Pagination.Page = Math.Max(1, this.Pagination.Page - 1);
            await LoadData();
        }

        private async void PART_BtnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            if (this.Pagination.Page == 1)
            {
                return;
            }
            this.Pagination.Page = 1;
            await LoadData();
        }


        private async void Pagination_OnRowsChanged(Pagination pagination)
        {
            await LoadData();
        }

        //public event Func<LoadDataArgs, Task<int>> OnLoadDataAsync;

        private async void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight)
            {
                await LoadData();
            }
        }

        private async Task LoadData()
        {

            if (this.LoadDataCTS != null)
            {
                await this.LoadDataCTS.CancelAsync();
            }

            this.LoadDataCTS = new CancellationTokenSource();
            LoadDataArgs loadMoreDataArgs = new LoadDataArgs();
            loadMoreDataArgs.Rows = this.Pagination.Rows;
            loadMoreDataArgs.Sord = this.Pagination.Sord;
            loadMoreDataArgs.Page = this.Pagination.Page;
            loadMoreDataArgs.Sidx = this.Pagination.Sidx;
            loadMoreDataArgs.CancellationToken = LoadDataCTS.Token;
            var loadDataCommand = this.LoadDataCommand;
            var pagination = this.Pagination;

            //可取消
            await this.PART_RSUserControl.InvokeLoadingActionAsync(async (cancellationToken) =>
            {
                if (loadDataCommand != null)
                {
                    var records = await loadDataCommand?.ExecuteAsync(loadMoreDataArgs);
                    pagination.Records = records;
                }
                return OperateResult.CreateSuccessResult();
            }, cancellationToken: LoadDataCTS.Token);

        }
    }
}
