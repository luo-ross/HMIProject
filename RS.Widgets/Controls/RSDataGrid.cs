using RS.Commons;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class RSDataGrid : ContentControl
    {
        private DataGrid PART_DataGrid;

        private RSUserControl PART_RSUserControl;
        static RSDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDataGrid), new FrameworkPropertyMetadata(typeof(RSDataGrid)));
        }

        public RSDataGrid()
        {
            this.Loaded += RSDataGrid_Loaded;
        }

        private void RSDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = this.PART_DataGrid.FindChild<ScrollViewer>();
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged -= OnScrollChanged;
                scrollViewer.ScrollChanged += OnScrollChanged;
            }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(RSDataGrid), new PropertyMetadata(null));


        public ObservableCollection<DataGridColumn> Columns
        {
            get { return (ObservableCollection<DataGridColumn>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(ObservableCollection<DataGridColumn>), typeof(RSDataGrid), new PropertyMetadata(new ObservableCollection<DataGridColumn>()));


        public double MinRowHeight
        {
            get { return (double)GetValue(MinRowHeightProperty); }
            set { SetValue(MinRowHeightProperty, value); }
        }

        public static readonly DependencyProperty MinRowHeightProperty =
            DependencyProperty.Register("MinRowHeight", typeof(double), typeof(RSDataGrid), new PropertyMetadata(28D));


        public Pagination Pagination
        {
            get { return (Pagination)GetValue(PaginationProperty); }
            set { SetValue(PaginationProperty, value); }
        }

        public static readonly DependencyProperty PaginationProperty =
            DependencyProperty.Register("Pagination", typeof(Pagination), typeof(RSDataGrid), new PropertyMetadata(new Pagination()));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_DataGrid = this.GetTemplateChild(nameof(this.PART_DataGrid)) as DataGrid;
            this.PART_RSUserControl = this.GetTemplateChild(nameof(this.PART_RSUserControl)) as RSUserControl;
            if (this.PART_DataGrid != null)
            {
                this.PART_DataGrid.Columns.Clear();
                foreach (var column in this.Columns)
                {
                    this.PART_DataGrid.Columns.Add(column);
                }
            }
        }


        public event Func<LoadMoreDataArgs, Task> LoadMoreDataAsync;
        private CancellationTokenSource LoadMoreDataCTS;
        private async void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight)
            {
                if (this.LoadMoreDataCTS != null)
                {
                    await this.LoadMoreDataCTS.CancelAsync();
                }
                this.LoadMoreDataCTS = new CancellationTokenSource();
                LoadMoreDataArgs loadMoreDataArgs = new LoadMoreDataArgs();
                loadMoreDataArgs.Total = this.Pagination.Total;
                loadMoreDataArgs.Rows = this.Pagination.Rows;
                loadMoreDataArgs.Records = this.Pagination.Records;
                loadMoreDataArgs.Sord = this.Pagination.Sord;
                loadMoreDataArgs.Page = this.Pagination.Page;
                loadMoreDataArgs.Sidx = this.Pagination.Sidx;
                loadMoreDataArgs.CancellationToken = LoadMoreDataCTS.Token;

                //可取消
                await this.PART_RSUserControl.InvokeLoadingActionAsync(async (cancellationToken) =>
                {
                    if (this.LoadMoreDataAsync != null)
                    {
                        await this.LoadMoreDataAsync.Invoke(loadMoreDataArgs);
                    }
                    return OperateResult.CreateSuccessResult();
                }, cancellationToken: LoadMoreDataCTS.Token);
            }
        }
    }
}
