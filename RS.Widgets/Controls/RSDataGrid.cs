using CommunityToolkit.Mvvm.Input;
using NPOI.POIFS.Crypt.Dsig.Services;
using RS.Commons;
using RS.Widgets.Enums;
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

namespace RS.Widgets.Controls
{
    public class RSDataGrid : DataGrid, ILoadingService
    {
        private DataGrid PART_DataGrid;
        private RSUserControl PART_RSUserControl;
        private Button PART_BtnFirstPage;
        private Button PART_BtnPrevious;
        private Button PART_BtnNext;
        private Button PART_BtnEndPage;

        public event Func<Pagination, Task> PaginationAsync;

        #region 路由事件

        public static readonly RoutedEvent AddClickEvent = EventManager.RegisterRoutedEvent(
            nameof(AddClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSDataGrid));
        public static readonly RoutedEvent DeleteClickEvent = EventManager.RegisterRoutedEvent(
             nameof(DeleteClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSDataGrid));
        public static readonly RoutedEvent UpdateClickEvent = EventManager.RegisterRoutedEvent(
            nameof(UpdateClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSDataGrid));
        public static readonly RoutedEvent DetailsClickEvent = EventManager.RegisterRoutedEvent(
             nameof(DetailsClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSDataGrid));
        public static readonly RoutedEvent ExportClickEvent = EventManager.RegisterRoutedEvent(
             nameof(ExportClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RSDataGrid));

        public event RoutedEventHandler AddClick
        {
            add { AddHandler(AddClickEvent, value); }
            remove { RemoveHandler(AddClickEvent, value); }
        }
        public event RoutedEventHandler DeleteClick
        {
            add { AddHandler(DeleteClickEvent, value); }
            remove { RemoveHandler(DeleteClickEvent, value); }
        }
        public event RoutedEventHandler UpdateClick
        {
            add { AddHandler(UpdateClickEvent, value); }
            remove { RemoveHandler(UpdateClickEvent, value); }
        }
        public event RoutedEventHandler DetailsClick
        {
            add { AddHandler(DetailsClickEvent, value); }
            remove { RemoveHandler(DetailsClickEvent, value); }
        }
        public event RoutedEventHandler ExportClick
        {
            add { AddHandler(ExportClickEvent, value); }
            remove { RemoveHandler(ExportClickEvent, value); }
        }
        #endregion

        #region 静态命令

        public static readonly ICommand InternalAddCommand = new RoutedCommand(nameof(InternalAddCommand), typeof(RSDataGrid), new InputGestureCollection
        {
            new KeyGesture(Key.A, ModifierKeys.Alt)
        });
        public static readonly ICommand InternalDeleteCommand = new RoutedCommand(nameof(InternalDeleteCommand), typeof(RSDataGrid), new InputGestureCollection
        {
            new KeyGesture(Key.D, ModifierKeys.Alt)
        });
        public static readonly ICommand InternalUpdateCommand = new RoutedCommand(nameof(InternalUpdateCommand), typeof(RSDataGrid), new InputGestureCollection
        {
            new KeyGesture(Key.U, ModifierKeys.Alt)
        });
        public static readonly ICommand InternalDetailsCommand = new RoutedCommand(nameof(InternalDetailsCommand), typeof(RSDataGrid), new InputGestureCollection
        {
            new KeyGesture(Key.V, ModifierKeys.Alt)
        });
        public static readonly ICommand InternalExportCommand = new RoutedCommand(nameof(InternalExportCommand), typeof(RSDataGrid), new InputGestureCollection
        {
            new KeyGesture(Key.E, ModifierKeys.Alt)
        });

        static RSDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDataGrid), new FrameworkPropertyMetadata(typeof(RSDataGrid)));

            CommandManager.RegisterClassCommandBinding(typeof(RSDataGrid),
                new CommandBinding(InternalAddCommand, InternalAddCommandExecuted, CanInternalAddCommandExecute));
            CommandManager.RegisterClassCommandBinding(typeof(RSDataGrid),
                new CommandBinding(InternalDeleteCommand, InternalDeleteCommandExecuted, CanInternalDeleteCommandExecute));
            CommandManager.RegisterClassCommandBinding(typeof(RSDataGrid),
                new CommandBinding(InternalUpdateCommand, InternalUpdateCommandExecuted, CanInternalUpdateCommandExecute));
            CommandManager.RegisterClassCommandBinding(typeof(RSDataGrid),
                new CommandBinding(InternalDetailsCommand, InternalDetailsCommandExecuted, CanInternalDetailsCommandExecute));
            CommandManager.RegisterClassCommandBinding(typeof(RSDataGrid),
                new CommandBinding(InternalExportCommand, InternalExportCommandExecuted, CanInternalExportCommandExecute));
        }
        #endregion


        private static void CanInternalExportCommandExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void InternalExportCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as RSDataGrid;
            if (dataGrid != null)
            {
                dataGrid.RaiseEvent(new RoutedEventArgs(ExportClickEvent, dataGrid.ItemsSource));
                dataGrid.ExportClickCommand?.Execute(dataGrid.ItemsSource);
            }
        }

        private static void CanInternalDetailsCommandExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void InternalDetailsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as RSDataGrid;
            if (dataGrid != null)
            {
                dataGrid.RaiseEvent(new RoutedEventArgs(DetailsClickEvent, dataGrid.SelectedItem));
                dataGrid.DetailsClickCommand?.Execute(dataGrid.SelectedItem);
            }
        }

        private static void CanInternalUpdateCommandExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void InternalUpdateCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as RSDataGrid;
            if (dataGrid != null)
            {
                dataGrid.RaiseEvent(new RoutedEventArgs(UpdateClickEvent, dataGrid.SelectedItem));
                dataGrid.UpdateClickCommand?.Execute(dataGrid.SelectedItem);
            }
        }

        private static void CanInternalDeleteCommandExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void InternalDeleteCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as RSDataGrid;
            if (dataGrid != null)
            {
                dataGrid.RaiseEvent(new RoutedEventArgs(DeleteClickEvent, dataGrid.SelectedItem));
                dataGrid.DeleteClickCommand?.Execute(dataGrid.SelectedItem);
            }
        }

        // 命令执行方法
        private static void InternalAddCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as RSDataGrid;
            if (dataGrid != null)
            {
                dataGrid.RaiseEvent(new RoutedEventArgs(AddClickEvent, dataGrid.ItemsSource));
                dataGrid.AddClickCommand?.Execute(dataGrid.ItemsSource);
            }
        }

        private static void CanInternalAddCommandExecute(object sender, CanExecuteRoutedEventArgs e)
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
            //var scrollViewer = this.PART_DataGrid.FindChild<ScrollViewer>();
            //if (scrollViewer != null)
            //{
            //    scrollViewer.ScrollChanged -= OnScrollChanged;
            //    scrollViewer.ScrollChanged += OnScrollChanged;
            //}
            await InternalPaginationAsync();
        }

        #region 自定义Command
        public static readonly DependencyProperty LoadDataCommandProperty =
        DependencyProperty.Register(nameof(PaginationCommand), typeof(AsyncRelayCommand<Pagination>), typeof(RSDataGrid), new PropertyMetadata(null));

        public AsyncRelayCommand<Pagination> PaginationCommand
        {
            get { return (AsyncRelayCommand<Pagination>)GetValue(LoadDataCommandProperty); }
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
            if (PaginationCommand != null)
            {
                return this.PaginationCommand.CanExecute(null);
            }
            else
            {
                return true;
            }
        }


        #endregion

        #region 只读Command事件

        // 新增数据命令依赖属性
        public ICommand AddClickCommand
        {
            get { return (ICommand)GetValue(AddCommandProperty); }
            set { SetValue(AddCommandProperty, value); }
        }

        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register(nameof(AddClickCommand), typeof(ICommand), typeof(RSDataGrid), new PropertyMetadata(null));



        // 删除选中命令依赖属性

        public ICommand DeleteClickCommand
        {
            get { return (ICommand)GetValue(DeleteClickCommandProperty); }
            set { SetValue(DeleteClickCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteClickCommandProperty =
            DependencyProperty.Register(nameof(DeleteClickCommand), typeof(ICommand), typeof(RSDataGrid), new PropertyMetadata(null));




        // 修改数据命令依赖属性
        public ICommand UpdateClickCommand
        {
            get { return (ICommand)GetValue(UpdateClickCommandProperty); }
            set { SetValue(UpdateClickCommandProperty, value); }
        }

        public static readonly DependencyProperty UpdateClickCommandProperty =
            DependencyProperty.Register(nameof(UpdateClickCommand), typeof(ICommand), typeof(RSDataGrid), new PropertyMetadata(null));




        // 查看数据命令依赖属性
        public ICommand DetailsClickCommand
        {
            get { return (ICommand)GetValue(DetailsClickCommandProperty); }
            set { SetValue(DetailsClickCommandProperty, value); }
        }

        public static readonly DependencyProperty DetailsClickCommandProperty =
            DependencyProperty.Register(nameof(DetailsClickCommand), typeof(ICommand), typeof(RSDataGrid), new PropertyMetadata(null));




        // 导出配置命令依赖属性
        public ICommand ExportClickCommand
        {
            get { return (ICommand)GetValue(ExportClickCommandProperty); }
            set { SetValue(ExportClickCommandProperty, value); }
        }

        public static readonly DependencyProperty ExportClickCommandProperty =
            DependencyProperty.Register(nameof(ExportClickCommand), typeof(ICommand), typeof(RSDataGrid), new PropertyMetadata(null));

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
            await InternalPaginationAsync();
        }

        private async void PART_BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (this.Pagination.Page == this.Pagination.Total)
            {
                return;
            }
            this.Pagination.Page = Math.Min(this.Pagination.Total, this.Pagination.Page + 1);
            await InternalPaginationAsync();
        }

        private async void PART_BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (this.Pagination.Page == 1)
            {
                return;
            }
            this.Pagination.Page = Math.Max(1, this.Pagination.Page - 1);
            await InternalPaginationAsync();
        }

        private async void PART_BtnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            if (this.Pagination.Page == 1)
            {
                return;
            }
            this.Pagination.Page = 1;
            await InternalPaginationAsync();
        }


        private async void Pagination_OnRowsChanged(Pagination pagination)
        {
            await InternalPaginationAsync();
        }


        //private async void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    if (e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight)
        //    {
        //        await InternalPaginationAsync();
        //    }
        //}

        private async Task InternalPaginationAsync()
        {
            var paginationCommand = this.PaginationCommand;
            var pagination = this.Pagination;
          
            if (paginationCommand != null)
            {
                await paginationCommand.ExecuteAsync(pagination);
            }
            else if (this.PaginationAsync != null)
            {
                await this.PaginationAsync.Invoke(pagination);
            }
            
        }

        public async Task<OperateResult> InvokeLoadingActionAsync(Func<CancellationToken, Task<OperateResult>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        {
            return await this.PART_RSUserControl.InvokeLoadingActionAsync(func, loadingConfig, cancellationToken);
        }

        public async Task<OperateResult<T>> InvokeLoadingActionAsync<T>(Func<CancellationToken, Task<OperateResult<T>>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        {
            return await this.PART_RSUserControl.InvokeLoadingActionAsync<T>(func, loadingConfig, cancellationToken);
        }
    }
}
