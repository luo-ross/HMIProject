using CommunityToolkit.Mvvm.Input;
using RS.Widgets.CustomEventArgs;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using RS.Widgets.Utilities;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class RSCalendar : ContentControl
    {

        #region Fields
        private Border PART_TitleHost;
        private Grid PART_DayOfWeekHost;
        private ScrollViewer PART_ScrollViewer;
        private Canvas? PART_Canvas;
        private Button? PART_BtnTitle;
        private Button? PART_PageUp;
        private Button? PART_PageDown;
        private Popup? Popup;

        private const int DayCols = 7;
        private const int DayRows = 6;
        private const int YearOrMonthCols = 4;
        private const int YearOrMonthRows = 4;
        private const double MinItenSize = 45D;

        /// <summary>
        /// 日历控件尺寸
        /// </summary>
        private double CalendarItemSizeShould;

        /// <summary>
        /// 标题栏高度
        /// </summary>
        private double HeaderHeight;

        /// <summary>
        /// 日历宽度
        /// </summary>
        private double CalendarWidth;

        /// <summary>
        /// 日历高度
        /// </summary>
        private double CalendarHeight;

        /// <summary>
        /// 初始化历史
        /// </summary>
        private DateTime? DateTimeInitHistory;

        /// <summary>
        /// 是否应该滚动到对应的偏移位置
        /// </summary>
        private bool IsShouldScrollToVerticalOffSet = true;

        /// <summary>
        /// 保存历史日历项数据
        /// </summary>
        private List<CalendarBaseModel> CalendarItemModelList = new List<CalendarBaseModel>();

        /// <summary>
        /// 当前年月的日期
        /// </summary>
        private DateTime? CurrentYearMonthDate;

        /// <summary>
        /// 当前年的日期
        /// </summary>
        private DateTime? CurrentYearDate;

        /// <summary>
        /// 是否允许滚动事件更改
        /// </summary>
        private bool IsScrollViewerScrollShouldChanged = true;

        /// <summary>
        /// 日历项点击命令
        /// </summary>
        private ICommand CalendarItemClickCommand;

        /// <summary>
        /// 语言项列表
        /// </summary>
        public static List<LanguageItem> LanguageItemList;
        #endregion

        static RSCalendar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSCalendar), new FrameworkPropertyMetadata(typeof(RSCalendar)));

            LanguageItemList = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(c => !string.IsNullOrEmpty(c.IetfLanguageTag)) // 过滤有效语言标签
                .OrderBy(c => c.DisplayName) // 按显示名称排序
                .Select(c => new LanguageItem
                {
                    DisplayName = c.DisplayName,
                    LanguageTag = c.IetfLanguageTag,
                    Culture = c
                }).ToList();
        }

        public RSCalendar()
        {
            this.InitializeWeekdays();
            CalendarItemClickCommand = new RelayCommand<CalendarBaseModel?>(CalendarItemClick);
            //设计模式下不执行事件
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.Loaded += RSCalendar_Loaded;
                this.SizeChanged += RSCalendar_SizeChanged;
            }
        }


        private void RSCalendar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //避免重复渲染
            if (!this.IsLoaded)
            {
                return;
            }
            this.UpdateCalendarView();
        }


        private void RSCalendar_Loaded(object sender, RoutedEventArgs e)
        {
            //加载完成后更新视图
            this.UpdateCalendarView();
            this.Popup = this.TryFindParent<Popup>();
        }


        #region 路由事件

        /// <summary>
        /// 日期选中路由事件
        /// </summary>
        public static readonly RoutedEvent DateSelectedEvent = EventManager.RegisterRoutedEvent(
            nameof(DateSelected),
            RoutingStrategy.Bubble,
            typeof(EventHandler<CalendarDateSelectedEventArgs>),
            typeof(RSCalendar));

        /// <summary>
        /// 日期选中事件
        /// </summary>
        public event EventHandler<CalendarDateSelectedEventArgs> DateSelected
        {
            add { AddHandler(DateSelectedEvent, value); }
            remove { RemoveHandler(DateSelectedEvent, value); }
        }

        #endregion

        #region 依赖属性

        /// <summary>
        /// 日期选中命令（用于MVVM绑定）
        /// </summary>
        public ICommand DateSelectedCommand
        {
            get { return (ICommand)GetValue(DateSelectedCommandProperty); }
            set { SetValue(DateSelectedCommandProperty, value); }
        }

        /// <summary>
        /// 日期选中命令依赖属性
        /// </summary>
        public static readonly DependencyProperty DateSelectedCommandProperty =
            DependencyProperty.Register(nameof(DateSelectedCommand), typeof(ICommand), typeof(RSCalendar), new PropertyMetadata(null));

        #endregion


        /// <summary>
        /// 日历项大小
        /// </summary>
        public double ItemSize
        {
            get { return (double)GetValue(ItemSizeProperty); }
            set { SetValue(ItemSizeProperty, value); }
        }

        public static readonly DependencyProperty ItemSizeProperty =
            DependencyProperty.Register(nameof(ItemSize), typeof(double), typeof(RSCalendar), new PropertyMetadata(MinItenSize));

        /// <summary>
        /// 星期高度
        /// </summary>
        public double DayOfWeekHeight
        {
            get { return (double)GetValue(DayOfWeekHeightProperty); }
            private set { SetValue(DayOfWeekHeightProperty, value); }
        }

        public static readonly DependencyProperty DayOfWeekHeightProperty =
            DependencyProperty.Register(nameof(DayOfWeekHeight), typeof(double), typeof(RSCalendar), new PropertyMetadata(MinItenSize));


        /// <summary>
        /// 日历视图类型
        /// </summary>
        public CalendarViewType CalendarViewType
        {
            get { return (CalendarViewType)GetValue(CalendarViewTypeProperty); }
            private set { SetValue(CalendarViewTypeProperty, value); }
        }

        public static readonly DependencyProperty CalendarViewTypeProperty =
            DependencyProperty.Register(nameof(CalendarViewType), typeof(CalendarViewType), typeof(RSCalendar), new PropertyMetadata(CalendarViewType.Day));


        /// <summary>
        /// 日历类型
        /// </summary>
        public CalendarSelectType CalendarSelectType
        {
            get { return (CalendarSelectType)GetValue(CalendarSelectTypeProperty); }
            set { SetValue(CalendarSelectTypeProperty, value); }
        }

        public static readonly DependencyProperty CalendarSelectTypeProperty =
            DependencyProperty.Register(nameof(CalendarSelectType), typeof(CalendarSelectType), typeof(RSCalendar), new PropertyMetadata(CalendarSelectType.Day, OnCalendarSelectTypePropertyChanged));

        private static void OnCalendarSelectTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var calendar = d as RSCalendar;
            if (calendar == null)
            {
                return;
            }
            switch (calendar.CalendarSelectType)
            {
                case CalendarSelectType.Day:
                    calendar.CalendarViewType = CalendarViewType.Day;
                    break;
                case CalendarSelectType.Month:
                    calendar.CalendarViewType = CalendarViewType.Month;
                    break;
                case CalendarSelectType.Year:
                    calendar.CalendarViewType = CalendarViewType.Year;
                    break;
            }
            calendar?.UpdateCalendarView();
        }






        /// <summary>
        /// 星期描述列表
        /// </summary>
        public List<string> WeekdayList
        {
            get { return (List<string>)GetValue(WeekdayListProperty); }
            private set { SetValue(WeekdayListProperty, value); }
        }

        public static readonly DependencyProperty WeekdayListProperty =
            DependencyProperty.Register(nameof(WeekdayList), typeof(List<string>), typeof(RSCalendar), new PropertyMetadata(null));

        /// <summary>
        /// 圆角大小
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(RSCalendar), new PropertyMetadata(default));


        /// <summary>
        /// 日历最小日期
        /// </summary>
        public DateTime MinDate
        {
            get { return (DateTime)GetValue(MinDateProperty); }
            set { SetValue(MinDateProperty, value); }
        }

        public static readonly DependencyProperty MinDateProperty =
            DependencyProperty.Register(nameof(MinDate), typeof(DateTime), typeof(RSCalendar), new PropertyMetadata(DateTime.MinValue, OnMinDatePropertyChanged));

        private static void OnMinDatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var calendar = d as RSCalendar;
            calendar?.UpdateCalendarView();
        }


        /// <summary>
        /// 日历最大日期
        /// </summary>
        public DateTime MaxDate
        {
            get { return (DateTime)GetValue(MaxDateProperty); }
            set { SetValue(MaxDateProperty, value); }
        }

        public static readonly DependencyProperty MaxDateProperty =
            DependencyProperty.Register(nameof(MaxDate), typeof(DateTime), typeof(RSCalendar), new PropertyMetadata(DateTime.MaxValue, OnMaxDatePropertyChanged));

        private static void OnMaxDatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var calendar = d as RSCalendar;
            calendar?.UpdateCalendarView();
        }

        /// <summary>
        /// 日期选中
        /// </summary>
        public DateTime? DateTimeSelected
        {
            get { return (DateTime?)GetValue(DateTimeSelectedProperty); }
            set { SetValue(DateTimeSelectedProperty, value); }
        }

        public static readonly DependencyProperty DateTimeSelectedProperty =
            DependencyProperty.Register(nameof(DateTimeSelected), typeof(DateTime?), typeof(RSCalendar), new PropertyMetadata(null));


        /// <summary>
        /// 获取或设置一个值，指示是否显示组标签。
        /// </summary>
        public bool IsGroupLabelVisible
        {
            get { return (bool)GetValue(IsGroupLabelVisibleProperty); }
            set { SetValue(IsGroupLabelVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsGroupLabelVisibleProperty =
            DependencyProperty.Register(nameof(IsGroupLabelVisible), typeof(bool), typeof(RSCalendar), new PropertyMetadata(true));


        /// <summary>
        /// 获取或设置一个值，指示是否启用显示范围外的日期。
        /// </summary>
        public bool IsOutOfScopeEnabled
        {
            get { return (bool)GetValue(IsOutOfScopeEnabledProperty); }
            set { SetValue(IsOutOfScopeEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsOutOfScopeEnabledProperty =
            DependencyProperty.Register(nameof(IsOutOfScopeEnabled), typeof(bool), typeof(RSCalendar), new PropertyMetadata(true));



        /// <summary>
        /// 语言文化信息
        /// </summary>
        public CultureInfo CultureInfo
        {
            get { return (CultureInfo)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }

        public static readonly DependencyProperty CultureInfoProperty =
            DependencyProperty.Register(nameof(CultureInfo), typeof(CultureInfo), typeof(RSCalendar), new PropertyMetadata(CultureInfo.CurrentCulture));


        /// <summary>
        /// 选择模式
        /// </summary>
        public SelectionMode SelectionMode
        {
            get { return (SelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register(nameof(SelectionMode), typeof(SelectionMode), typeof(RSCalendar), new PropertyMetadata(SelectionMode.Single));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PART_TitleHost = this.GetTemplateChild(nameof(this.PART_TitleHost)) as Border;
            this.PART_BtnTitle = this.GetTemplateChild(nameof(this.PART_BtnTitle)) as Button;
            this.PART_PageUp = this.GetTemplateChild(nameof(this.PART_PageUp)) as Button;
            this.PART_PageDown = this.GetTemplateChild(nameof(this.PART_PageDown)) as Button;
            this.PART_DayOfWeekHost = this.GetTemplateChild(nameof(this.PART_DayOfWeekHost)) as Grid;
            this.PART_ScrollViewer = this.GetTemplateChild(nameof(this.PART_ScrollViewer)) as ScrollViewer;
            this.PART_Canvas = this.GetTemplateChild(nameof(this.PART_Canvas)) as Canvas;

            if (this.PART_ScrollViewer != null)
            {
                this.PART_ScrollViewer.ScrollChanged -= PART_ScrollViewer_ScrollChanged;
                this.PART_ScrollViewer.ScrollChanged += PART_ScrollViewer_ScrollChanged;
            }


            if (this.PART_BtnTitle != null)
            {
                this.PART_BtnTitle.Click += PART_BtnTitle_Click;
            }

            if (this.PART_PageUp != null)
            {
                this.PART_PageUp.Click += PART_PageUp_Click;
            }

            if (this.PART_PageDown != null)
            {
                this.PART_PageDown.Click += PART_PageDown_Click;
            }



        }

        private void PART_BtnTitle_Click(object sender, RoutedEventArgs e)
        {
            if (this.PART_BtnTitle == null)
            {
                return;
            }

            switch (this.CalendarSelectType)
            {
                case CalendarSelectType.Day:
                    switch (this.CalendarViewType)
                    {
                        case CalendarViewType.Day:
                            this.CalendarViewType = CalendarViewType.Month;
                            break;
                        case CalendarViewType.Month:
                            this.CalendarViewType = CalendarViewType.Year;
                            break;
                        case CalendarViewType.Year:
                            break;
                    }
                    break;
                case CalendarSelectType.Month:
                    switch (this.CalendarViewType)
                    {
                        case CalendarViewType.Month:
                            this.CalendarViewType = CalendarViewType.Year;
                            break;
                        case CalendarViewType.Year:
                            break;
                    }
                    break;
                case CalendarSelectType.Year:
                    break;
            }

            this.UpdateCalendarView();
        }

        private void PART_PageUp_Click(object sender, RoutedEventArgs e)
        {
            switch (this.CalendarViewType)
            {
                case CalendarViewType.Day:

                    var dateTimeInitHistory = this.GetDateTimeInitHistory();
                    this.DateTimeInitHistory = new DateTime(dateTimeInitHistory.Year, dateTimeInitHistory.Month, 1).AddMonths(-1);
                    break;
                case CalendarViewType.Month:
                    this.CurrentYearMonthDate = this.GetCurrentYearMonthDate().AddYears(-1);
                    break;
                case CalendarViewType.Year:
                    this.CurrentYearDate = this.GetCurrentYearDate().AddYears(-10);
                    break;
                default:
                    break;
            }

            UpdateCalendarView();
        }

        private void PART_PageDown_Click(object sender, RoutedEventArgs e)
        {
            switch (this.CalendarViewType)
            {
                case CalendarViewType.Day:
                    var dateTimeInitHistory = this.GetDateTimeInitHistory();
                    this.DateTimeInitHistory = new DateTime(dateTimeInitHistory.Year, dateTimeInitHistory.Month, 1).AddMonths(1);
                    break;
                case CalendarViewType.Month:
                    this.CurrentYearMonthDate = this.GetCurrentYearMonthDate().AddYears(1);
                    break;
                case CalendarViewType.Year:
                    this.CurrentYearDate = this.GetCurrentYearDate().AddYears(10);
                    break;
                default:
                    break;
            }

            UpdateCalendarView();
        }


        private DateTime GetCurrentYearDate()
        {
            if (!this.CurrentYearDate.HasValue)
            {
                var dateTimeInitHistory = this.GetDateTimeInitHistory();
                return new DateTime(dateTimeInitHistory.Year, 1, 1);
            }

            return this.CurrentYearDate.Value;
        }


        private DateTime GetCurrentYearMonthDate()
        {
            if (!this.CurrentYearMonthDate.HasValue)
            {
                var dateTimeInitHistory = this.GetDateTimeInitHistory();
                return new DateTime(dateTimeInitHistory.Year, dateTimeInitHistory.Month, 1);
            }

            return this.CurrentYearMonthDate.Value;
        }


        private void PART_ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!this.IsScrollViewerScrollShouldChanged)
            {
                return;
            }

            if (!this.PART_ScrollViewer.IsMouseOver)
            {
                return;
            }


            this.IsShouldScrollToVerticalOffSet = false;

            var verticalOffset = e.VerticalOffset;

            switch (this.CalendarViewType)
            {
                case CalendarViewType.Day:
                    if (!this.DateTimeInitHistory.HasValue)
                    {
                        this.DateTimeInitHistory = DateTime.Now;
                    }
                    else
                    {
                        //计算得到在第几行
                        var row = (int)(verticalOffset / this.CalendarItemSizeShould);
                        //计算得到日期的索引
                        var dayIndex = row * DayCols;
                        dayIndex = GetActualDayIndex(dayIndex);
                        this.DateTimeInitHistory = GetDateFromDayIndex(dayIndex);
                    }
                    break;
                case CalendarViewType.Month:

                    if (!this.CurrentYearMonthDate.HasValue)
                    {
                        var dateTimeNow = DateTime.Now;
                        this.CurrentYearMonthDate = new DateTime(dateTimeNow.Year, dateTimeNow.Month, 1);
                    }
                    else
                    {
                        //计算得到在第几行
                        var row = (int)(verticalOffset / this.CalendarItemSizeShould);
                        //计算得到日期的索引
                        var monthIndex = row * YearOrMonthCols;
                        this.CurrentYearMonthDate = GetDateFromMonthIndex(monthIndex);
                    }

                    break;
                case CalendarViewType.Year:
                    if (!this.CurrentYearDate.HasValue)
                    {
                        var dateTimeNow = DateTime.Now;
                        this.CurrentYearDate = new DateTime(dateTimeNow.Year, 1, 1);
                    }
                    else
                    {
                        //计算得到在第几行
                        var row = (int)(verticalOffset / this.CalendarItemSizeShould);
                        //计算得到日期的索引
                        var yearIndex = row * YearOrMonthCols;
                        this.CurrentYearDate = GetDateFromYearIndex(yearIndex);
                    }
                    break;
            }

            this.UpdateCalendarView();
        }


        private void UpdateCalendarView()
        {
            if (this.PART_Canvas == null)
            {
                return;
            }

            #region 计算日历控件尺寸
            var itemSize = Math.Max(MinItenSize, this.ItemSize);

            this.CalendarWidth = this.ActualWidth;
            this.CalendarHeight = this.ActualHeight;


            var scrollViewerWidth = this.PART_ScrollViewer.ActualWidth;
            var scrollViewerHeight = this.PART_ScrollViewer.ActualHeight;

            var dayOfWeekHostActualWidth = this.PART_DayOfWeekHost.ActualWidth;
            var dayOfWeekHostActualHeight = this.PART_DayOfWeekHost.ActualHeight;


            this.HeaderHeight = this.PART_TitleHost.ActualHeight;

            //如果用户设置HorizontalAlignment为Stretch 则说明需要宽自适应
            if (this.HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                var calendarWidth = itemSize * DayCols;
                //必须保证最小尺寸
                if (this.CalendarWidth < calendarWidth)
                {
                    if (this.Width != calendarWidth)
                    {
                        this.Width = calendarWidth;
                        //这里因为改变了控件的尺寸会触发SizeChanged事件
                        return;
                    }
                    this.CalendarWidth = calendarWidth;
                }
            }

            itemSize = Math.Max(itemSize, this.CalendarWidth / DayCols);

            var height = this.Height;
            //如果用户设置VerticalAlignment为Stretch 则说明需要高自适应
            if (this.VerticalAlignment != VerticalAlignment.Stretch)
            {
                var calendarHeight = itemSize * DayRows + this.DayOfWeekHeight + this.HeaderHeight;
                if (this.CalendarHeight < calendarHeight)
                {
                    this.Height = calendarHeight;
                    this.CalendarHeight = calendarHeight;
                    if (height != calendarHeight)
                    {
                        //这里因为改变了控件的尺寸会触发SizeChanged事件
                        return;
                    }
                }
            }
            else
            {
                DependencyObject parent = VisualTreeHelper.GetParent(this);
                //如果父元素是ScrollViewer 则说明需要计算滚动条宽度
                if (parent is ScrollViewer && scrollViewerHeight > 0)
                {
                    this.Height = this.CalendarHeight;
                    if (height != this.CalendarHeight)
                    {
                        //这里因为改变了控件的尺寸会触发SizeChanged事件
                        return;
                    }
                }
            }
            #endregion


            switch (this.CalendarViewType)
            {
                case CalendarViewType.Month:
                case CalendarViewType.Year:
                    //根据日控件的尺寸得到月和年的尺寸
                    itemSize = itemSize * DayCols / YearOrMonthCols;
                    break;
            }
            CalendarItemSizeShould = itemSize;


            //获得总年数 总月数 总天数
            var totalYears = this.MaxDate.Year - this.MinDate.Year + 1;
            var totalMonths = totalYears * 12;
            var totalDays = (this.MaxDate - this.MinDate).TotalDays;

            int totalItems = 0;
            int dataCols = 0;
            int dataIndex = -1;
            DateTime dateTimeInit = DateTime.Now;
            switch (this.CalendarViewType)
            {
                case CalendarViewType.Day:
                    dateTimeInit = this.GetDateTimeInitHistory();
                    totalItems = (int)totalDays;
                    dataCols = DayCols;
                    dataIndex = GetDayIndex(dateTimeInit);
                    break;
                case CalendarViewType.Month:
                    dateTimeInit = this.GetCurrentYearMonthDate();
                    totalItems = totalMonths;
                    dataCols = YearOrMonthCols;
                    dataIndex = GetMonthIndex(dateTimeInit);
                    break;
                case CalendarViewType.Year:
                    dateTimeInit = this.GetCurrentYearDate();
                    totalItems = totalYears;
                    dataCols = YearOrMonthCols;
                    dataIndex = GetYearIndex(dateTimeInit);
                    break;
            }
            if (totalItems == 0 || dataCols == 0 || dataIndex == -1)
            {
                return;
            }


            //获取天的总行数 
            var totalRows = (int)Math.Ceiling((double)totalItems / dataCols);

            //得到总高度
            var totalItemHeightShould = totalRows * CalendarItemSizeShould;

            this.PART_Canvas.Height = totalItemHeightShould;


            //计算得出视窗要显示多少行
            int rowDisplayShould = (int)Math.Ceiling((this.CalendarHeight - this.HeaderHeight) / this.CalendarItemSizeShould);
            switch (this.CalendarViewType)
            {
                case CalendarViewType.Day:
                    rowDisplayShould = (int)Math.Ceiling((this.CalendarHeight - this.HeaderHeight - this.CalendarItemSizeShould) / this.CalendarItemSizeShould);
                    break;
            }

            rowDisplayShould = Math.Max(rowDisplayShould, YearOrMonthRows);


            //日在第几行
            int dataRowIndex = dataIndex / dataCols;

            if (this.IsShouldScrollToVerticalOffSet)
            {
                //滚动到对应位置
                this.PART_ScrollViewer.ScrollToVerticalOffset(dataRowIndex * CalendarItemSizeShould);
            }

            int firstRow = Math.Max(dataRowIndex - 1, 0);
            int lastRow = Math.Min(dataRowIndex + rowDisplayShould + 1, totalRows);

            //这里要对最后一行做处理 我们必须保证1个视窗至少显示rowDisplayShould
            var actualRows = lastRow - firstRow;
            if (actualRows < rowDisplayShould + 2)
            {
                firstRow = Math.Max(dataRowIndex - (rowDisplayShould + 2 - actualRows), 0);
            }

            var dateTimeNow = DateTime.Now;
            var currenDay = dateTimeNow.Day;
            var currentMonth = dateTimeNow.Month;
            var currentYear = dateTimeNow.Year;


            //生成数据
            List<CalendarBaseModel> dataList = new List<CalendarBaseModel>();
            for (int row = firstRow; row < lastRow; row++)
            {
                for (int col = 0; col < dataCols; col++)
                {
                    dataIndex = row * dataCols + col;
                    if (dataIndex < 0)
                    {
                        continue;
                    }
                    if (dataIndex > totalItems)
                    {
                        continue;
                    }

                    switch (this.CalendarViewType)
                    {
                        case CalendarViewType.Day:
                            {
                                //日需要做特殊处理
                                var dayOfWeek = (int)this.MinDate.DayOfWeek;
                                if (dataIndex < dayOfWeek)
                                {
                                    continue;
                                }

                                var date = this.GetDateFromDayIndex(dataIndex);
                                dataList.Add(new CalendarDayModel()
                                {
                                    Date = date,
                                    DisplayContent = date.Day.ToString(),
                                    IsCurrentMonth = date.Month == currentMonth && date.Year == currentYear,
                                    IsToday = date.Month == currentMonth && date.Year == currentYear && date.Day == currenDay,
                                    IsFirstDayOfMonth = date.Day == 1,
                                    MonthName = this.CultureInfo.DateTimeFormat.GetMonthName(date.Month),
                                    IsSelected = false,
                                    IsBlackout = false,
                                    CalendarItemClickCommand = this.CalendarItemClickCommand
                                });
                            }
                            break;
                        case CalendarViewType.Month:
                            {
                                var date = this.GetDateFromMonthIndex(dataIndex);
                                dataList.Add(new CalendarMonthModel()
                                {
                                    Date = date,
                                    DisplayContent = this.CultureInfo.DateTimeFormat.GetAbbreviatedMonthName(date.Month),
                                    IsCurrentMonth = date.Month == currentMonth && date.Year == currentYear,
                                    IsFirstMonthOfYear = date.Month == 1,
                                    Year = date.Year,
                                    IsSelected = false,
                                    IsBlackout = false,
                                    CalendarItemClickCommand = this.CalendarItemClickCommand
                                });
                            }
                            break;
                        case CalendarViewType.Year:
                            {
                                var date = this.GetDateFromYearIndex(dataIndex);
                                dataList.Add(new CalendarYearModel()
                                {
                                    Date = date,
                                    DisplayContent = $"{date.Year}",
                                    IsCurrentYear = date.Year == currentYear,
                                    IsSelected = false,
                                    IsBlackout = false,
                                    CalendarItemClickCommand = this.CalendarItemClickCommand
                                });
                            }
                            break;
                    }
                }
            }


            //加工数据
            var exceptList = CalendarItemModelList.Except(dataList).ToList();
            foreach (var item in exceptList)
            {
                CalendarItemModelList.Remove(item);
                this.PART_Canvas.Children.OfType<ContentControl>()
                    .Where(t => t.Content == item)
                    .ToList()
                    .ForEach(t => this.PART_Canvas.Children.Remove(t));
            }
            //再求差集
            var newList = dataList.Except(CalendarItemModelList).ToList();
            //添加新的数据
            CalendarItemModelList.AddRange(newList);
            //对已有的数据进行更新
            switch (this.CalendarViewType)
            {
                case CalendarViewType.Day:
                    this.SetCalendarItemModelSelected(CalendarItemModelList);
                    //设置IsBlackout属性
                    this.SetIsBlackoutByMonthCount(CalendarItemModelList);
                    break;
                case CalendarViewType.Month:
                    this.SetIsBlackoutByYearCount(CalendarItemModelList);
                    break;
                case CalendarViewType.Year:
                    break;
            }


            //我们这里直接添加新的
            foreach (var calendarItemModel in newList)
            {
                var date = calendarItemModel.Date;
                int dataColIndex = 0;
                switch (this.CalendarViewType)
                {
                    case CalendarViewType.Day:
                        dataIndex = GetDayIndex(date);
                        //在第几列
                        dataColIndex = (int)date.DayOfWeek;
                        break;
                    case CalendarViewType.Month:
                        dataIndex = GetMonthIndex(date);
                        //在第几列
                        dataColIndex = (date.Month - 1) % YearOrMonthCols;
                        break;
                    case CalendarViewType.Year:
                        dataIndex = GetYearIndex(date);
                        //在第几列
                        dataColIndex = dataIndex % YearOrMonthCols;
                        break;
                }

                //计算在第几行
                dataRowIndex = dataIndex / dataCols;
                ContentControl contentControl = new ContentControl()
                {
                    Content = calendarItemModel,
                    Width = CalendarItemSizeShould,
                    Height = CalendarItemSizeShould,
                };
                Canvas.SetLeft(contentControl, dataColIndex * CalendarItemSizeShould);
                Canvas.SetTop(contentControl, dataRowIndex * CalendarItemSizeShould);
                this.PART_Canvas.Children.Add(contentControl);
            }

            //更新标题
            this.UpdateCalendarTitle();

            //重置滚动标记
            this.IsScrollViewerScrollShouldChanged = true;
            this.IsShouldScrollToVerticalOffSet = true;
        }

        private void UpdateCalendarTitle()
        {
            var calendarItemModel = this.CalendarItemModelList.FirstOrDefault(t => !t.IsBlackout);
            if (calendarItemModel == null)
            {
                return;
            }
            string title = string.Empty;
            switch (this.CalendarViewType)
            {
                case CalendarViewType.Day:
                    if (this.CultureInfo.Name.StartsWith("zh"))
                    {
                        title = calendarItemModel.Date.ToString("yyyy年M月", this.CultureInfo);
                    }
                    else
                    {
                        title = calendarItemModel.Date.ToString("MMMM yyyy", this.CultureInfo);
                    }
                    CurrentYearMonthDate = new DateTime(calendarItemModel.Date.Year, calendarItemModel.Date.Month, 1);
                    break;
                case CalendarViewType.Month:
                    title = calendarItemModel.Date.ToString("yyyy");
                    break;
                case CalendarViewType.Year:
                    var firstOrDefault = this.CalendarItemModelList.FirstOrDefault();
                    var lastOrDefault = this.CalendarItemModelList.LastOrDefault();
                    title = $"{firstOrDefault.Date.Year}-{lastOrDefault.Date.Year}";
                    break;
            }
            if (this.PART_BtnTitle != null)
            {
                this.PART_BtnTitle.Content = title;
            }
        }

        private void SetCalendarItemModelSelected(List<CalendarBaseModel> calendarItemModelList)
        {
            if (this.DateTimeSelected == null)
            {
                return;
            }

            var calendarItemModelSelectedHitory = calendarItemModelList.FirstOrDefault(item => item.Date == DateTimeSelected.Value);

            if (calendarItemModelSelectedHitory == null)
            {
                return;
            }

            calendarItemModelSelectedHitory.IsSelected = true;
        }


        private int GetYearIndex(DateTime dateTime)
        {
            return dateTime.Year - this.MinDate.Year;
        }

        private int GetMonthIndex(DateTime dateTime)
        {
            var yearDif = dateTime.Year - this.MinDate.Year;
            var monthIndex = yearDif * 12 + dateTime.Month - 1;
            return monthIndex;
        }


        private int GetDayIndex(DateTime dateTime)
        {
            var dayIndex = (int)(dateTime - this.MinDate).TotalDays;
            return GetActualDayIndex(dayIndex);
        }

        private int GetActualDayIndex(int dayIndex)
        {
            //因为1行7天，根据最小年第一天是星期几，日期的索引需要加上偏移
            dayIndex = dayIndex + (int)this.MinDate.DayOfWeek;
            return dayIndex;
        }


        public DateTime GetDateFromYearIndex(int yearIndex)
        {
            DateTime resultDate = this.MinDate.AddYears(yearIndex);
            return resultDate;
        }

        public DateTime GetDateFromMonthIndex(int monthIndex)
        {
            DateTime resultDate = this.MinDate.AddMonths(monthIndex);
            return resultDate;
        }

        public DateTime GetDateFromDayIndex(int dayIndex)
        {
            //这里需要减去偏移
            int originalDayIndex = dayIndex - (int)this.MinDate.DayOfWeek;

            if (originalDayIndex < 0 || originalDayIndex > (this.MaxDate - this.MinDate).TotalDays)
            {
                throw new ArgumentOutOfRangeException(nameof(dayIndex), "dayIndex is out of DateTime valid range");
            }
            DateTime resultDate = this.MinDate.AddDays(originalDayIndex);
            return resultDate;
        }


        /// <summary>
        /// 根据年份出现次数设置IsBlackout属性
        /// 出现次数最多的年份设置为false，其他年份设置为true
        /// </summary>
        /// <param name="calendarItemModelList">日历项列表</param>
        private void SetIsBlackoutByYearCount(List<CalendarBaseModel> calendarItemModelList)
        {
            if (calendarItemModelList == null || calendarItemModelList.Count == 0)
            {
                return;
            }

            var yearCountList = calendarItemModelList.Select(t => t.Date.Year).GroupBy(t => t).Select(t => new { Year = t.Key, Count = t.Count() }).ToList();

            int maxCount = yearCountList.Max(t => t.Count);
            var maxCountYears = yearCountList.Where(t => t.Count == maxCount).ToList();
            int maxYear = maxCountYears.First().Year;

            //设置IsBlackout：出现次数最多的年份为false，其他为true
            foreach (var item in calendarItemModelList)
            {
                item.IsBlackout = item.Date.Year != maxYear;
            }
        }


        /// <summary>
        /// 根据月份出现次数设置IsBlackout属性
        /// 出现次数最多的月份设置为false，其他月份设置为true
        /// </summary>
        /// <param name="calendarItemModelList">日历项列表</param>
        private void SetIsBlackoutByMonthCount(List<CalendarBaseModel> calendarItemModelList)
        {
            if (calendarItemModelList == null || calendarItemModelList.Count == 0)
            {
                return;
            }

            //创建12个元素的数组，每个索引对应月份（索引0对应1月，索引11对应12月）
            int[] monthCountArray = new int[12];

            //遍历日期，统计每个月份的出现次数
            foreach (var item in calendarItemModelList)
            {
                int monthIndex = item.Date.Month - 1;
                monthCountArray[monthIndex]++;
            }
            var month = Array.IndexOf(monthCountArray, monthCountArray.Max()) + 1;

            //设置IsBlackout：出现次数最多的月份为false，其他为true
            foreach (var item in calendarItemModelList)
            {
                item.IsBlackout = item.Date.Month != month;
            }
        }

        private void CalendarItemClick(CalendarBaseModel? calendarItemModel)
        {
            if (calendarItemModel == null)
            {
                return;
            }

            //清除之前的选中状态
            this.ClearSelectedState();
            calendarItemModel.IsSelected = true;

            switch (this.CalendarSelectType)
            {
                case CalendarSelectType.Day:
                    switch (this.CalendarViewType)
                    {
                        case CalendarViewType.Day:
                            RaiseDateTimeSelectEvent(calendarItemModel);
                            break;
                        case CalendarViewType.Month:
                            NavigateToDayView(calendarItemModel);
                            break;
                        case CalendarViewType.Year:
                            NavigateToMonthView(calendarItemModel);
                            break;
                    }
                    break;
                case CalendarSelectType.Month:
                    switch (this.CalendarViewType)
                    {
                        case CalendarViewType.Month:
                            RaiseDateTimeSelectEvent(calendarItemModel);
                            break;
                        case CalendarViewType.Year:
                            NavigateToMonthView(calendarItemModel);
                            break;
                    }
                    break;
                case CalendarSelectType.Year:
                    RaiseDateTimeSelectEvent(calendarItemModel);
                    break;
            }
        }

        private void RaiseDateTimeSelectEvent(CalendarBaseModel calendarItemModel)
        {
            if (this.Popup != null)
            {
                this.Popup.SetCurrentValue(Popup.IsOpenProperty, false);
            }

            DateTimeSelected = calendarItemModel.Date;
            //触发路由事件
            CalendarDateSelectedEventArgs eventArgs = new CalendarDateSelectedEventArgs(
                DateSelectedEvent,
                calendarItemModel.Date,
                calendarItemModel);
            RaiseEvent(eventArgs);
            //执行Command（如果设置了）
            if (DateSelectedCommand != null && DateSelectedCommand.CanExecute(calendarItemModel))
            {
                DateSelectedCommand.Execute(calendarItemModel);
            }
        }


        private void NavigateToDayView(CalendarBaseModel calendarItemModel)
        {
            this.IsScrollViewerScrollShouldChanged = false;
            this.DateTimeInitHistory = new DateTime(calendarItemModel.Date.Year, calendarItemModel.Date.Month, 1);
            this.CalendarViewType = CalendarViewType.Day;
            this.UpdateCalendarView();
            this.IsScrollViewerScrollShouldChanged = true;
        }

        private void NavigateToMonthView(CalendarBaseModel calendarItemModel)
        {
            this.IsScrollViewerScrollShouldChanged = false;
            this.CurrentYearMonthDate = new DateTime(calendarItemModel.Date.Year, 1, 1);
            this.CalendarViewType = CalendarViewType.Month;
            this.UpdateCalendarView();
        }

        private void ClearSelectedState()
        {
            foreach (var item in CalendarItemModelList)
            {
                item.IsSelected = false;
            }
        }

        private DateTime GetDateTimeInitHistory()
        {
            if (!DateTimeInitHistory.HasValue)
            {
                DateTimeInitHistory = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            return DateTimeInitHistory.Value;
        }

        private void InitializeWeekdays()
        {
            this.WeekdayList = this.CultureInfo.DateTimeFormat.ShortestDayNames.ToList();
        }
    }
}
