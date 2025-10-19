using RS.Widgets.Controls;
using RS.Widgets.Enums;
using RS.Widgets.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RS.Widgets.Controls
{
    public class RSDateTimePicker : ContentControl
    {

        private RSPopup PART_Popup;
        private Border PART_Border;
        private ToggleButton PART_BtnDatePicker;
        private Grid PART_PopupHost;
        private Button PART_BtnConfirm;
        private Button PART_BtnCancel;

        private bool IsCanUpdateDateTimeSelected = true;
        private bool IsCanUpdateFormattedDateTime = true;

        private int MinYear = DateTime.MinValue.Year;
        private int MaxYear = DateTime.MaxValue.Year;
        static RSDateTimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDateTimePicker), new FrameworkPropertyMetadata(typeof(RSDateTimePicker)));
        }

        public RSDateTimePicker()
        {
            this.RefreshYearPicker();
            this.Loaded += RSDateTimePicker_Loaded;
            this.SizeChanged += RSDateTimePicker_SizeChanged;
        }


        private void RSDateTimePicker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdatePopupSize();
        }

        private void RSDateTimePicker_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void PART_Popup_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdatePopupSize();
        }

        public DateTime MinDateTime
        {
            get { return (DateTime)GetValue(MinDateTimeProperty); }
            set { SetValue(MinDateTimeProperty, value); }
        }

        public static readonly DependencyProperty MinDateTimeProperty =
            DependencyProperty.Register("MinDateTime", typeof(DateTime), typeof(RSDateTimePicker), new PropertyMetadata(DateTime.MinValue, OnMinDateTimePropertyChanged));

        private static void OnMinDateTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //这里第一次不会触发
            var rsDateTimePicker = d as RSDateTimePicker;
            rsDateTimePicker.MinYear = rsDateTimePicker.MinDateTime.Year;
            rsDateTimePicker.RefreshYearPicker();
        }

        public DateTime MaxDateTime
        {
            get { return (DateTime)GetValue(MaxDateTimeProperty); }
            set { SetValue(MaxDateTimeProperty, value); }
        }

        public static readonly DependencyProperty MaxDateTimeProperty =
            DependencyProperty.Register("MaxDateTime", typeof(DateTime), typeof(RSDateTimePicker), new PropertyMetadata(DateTime.MaxValue, OnMaxDateTimePropertyChanged));

        private static void OnMaxDateTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //这里第一次不会触发
            var rsDateTimePicker = d as RSDateTimePicker;
            rsDateTimePicker.MaxYear = rsDateTimePicker.MaxDateTime.Year;
            rsDateTimePicker.RefreshYearPicker();
        }

        private void RefreshYearPicker()
        {
            List<int> yearList = new List<int>();
            for (int i = this.MinYear; i <= this.MaxYear; i++)
            {
                yearList.Add(i);
            }
            this.YearList = new ObservableCollection<int>(yearList);

            var defaultYear = this.YearSelected;
            //首相尝试使用默认值
            if (defaultYear == null)
            {
                if (this.DateTimeSelected.HasValue)
                {
                    defaultYear = this.DateTimeSelected.Value.Year;
                }
            }
            //确保有默认值
            if (defaultYear == null)
            {
                defaultYear = DateTime.Now.Year;
            }

            if (!this.YearList.Contains(defaultYear.Value))
            {
                //这里将日期清空
                this.DateTimeSelected = null;
                defaultYear = this.YearList.FirstOrDefault();
            }
            this.ForcePropertyChanged(YearSelectedProperty, this.YearSelected, defaultYear);
        }



        [Description("圆角大小")]
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(RSDateTimePicker), new PropertyMetadata(new CornerRadius(5)));


        [Description("年")]
        public ObservableCollection<int> YearList
        {
            get { return (ObservableCollection<int>)GetValue(YearListProperty); }
            set { SetValue(YearListProperty, value); }
        }

        public static readonly DependencyProperty YearListProperty =
            DependencyProperty.Register("YearList", typeof(ObservableCollection<int>), typeof(RSDateTimePicker), new PropertyMetadata(null));


        [Description("年选择")]
        public int? YearSelected
        {
            get { return (int?)GetValue(YearSelectedProperty); }
            set { SetValue(YearSelectedProperty, value); }
        }

        public static readonly DependencyProperty YearSelectedProperty =
            DependencyProperty.Register("YearSelected", typeof(int?), typeof(RSDateTimePicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnYearSelectedPropertyChanged));

        private static void OnYearSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsDateTimePicker = d as RSDateTimePicker;
            rsDateTimePicker.RefreshMonthPicker();
        }


        private void RefreshMonthPicker()
        {
            //判断年是否是最小值或者最大值
            List<int> monthList = new List<int>();
            if (this.YearSelected == this.MinDateTime.Year)
            {
                for (int i = this.MinDateTime.Month; i <= 12; i++)
                {
                    monthList.Add(i);
                }
            }
            else if (this.YearSelected == this.MaxDateTime.Year)
            {
                for (int i = 1; i <= this.MaxDateTime.Month; i++)
                {
                    monthList.Add(i);
                }
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    monthList.Add(i);
                }
            }

            this.MonthList = new ObservableCollection<int>(monthList);

            var defaultMonth = this.MonthSelected;
            //首相尝试使用默认值
            if (defaultMonth == null)
            {
                if (this.DateTimeSelected.HasValue)
                {
                    defaultMonth = this.DateTimeSelected.Value.Month;
                }
            }
            //确保有默认值
            if (defaultMonth == null)
            {
                defaultMonth = DateTime.Now.Month;
            }

            if (!this.MonthList.Contains(defaultMonth.Value))
            {
                //这里将日期清空
                this.DateTimeSelected = null;
                defaultMonth = this.MonthList.FirstOrDefault();
            }

            //主动通知
            this.ForcePropertyChanged(MonthSelectedProperty, this.MonthSelected, defaultMonth);
        }


        [Description("月")]
        public ObservableCollection<int> MonthList
        {
            get { return (ObservableCollection<int>)GetValue(MonthListProperty); }
            set { SetValue(MonthListProperty, value); }
        }

        public static readonly DependencyProperty MonthListProperty =
            DependencyProperty.Register("MonthList", typeof(ObservableCollection<int>), typeof(RSDateTimePicker), new PropertyMetadata(null));

        [Description("月选择")]
        public int? MonthSelected
        {
            get { return (int?)GetValue(MonthSelectedProperty); }
            set { SetValue(MonthSelectedProperty, value); }
        }

        public static readonly DependencyProperty MonthSelectedProperty =
            DependencyProperty.Register("MonthSelected", typeof(int?), typeof(RSDateTimePicker), new PropertyMetadata(null, OnMonthSelectedPropertyChanged));

        private static void OnMonthSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsDateTimePicker = d as RSDateTimePicker;
            rsDateTimePicker.RefreshDayPicker();
        }

        private void RefreshDayPicker()
        {
            if (this.YearSelected == null || this.MonthSelected == null)
            {
                return;
            }
            var days = DateTime.DaysInMonth(this.YearSelected.Value, this.MonthSelected.Value);
            List<int> dayList = new List<int>();


            //如果用户选择的刚好是最小日期

            if (this.YearSelected == this.MinDateTime.Year
                && this.MonthSelected == this.MinDateTime.Month)
            {
                for (int i = this.MinDateTime.Day; i <= days; i++)
                {
                    dayList.Add(i);
                }
            }
            else if (this.YearSelected == this.MaxDateTime.Year
                && this.MonthSelected == this.MaxDateTime.Month)
            {
                for (int i = 1; i <= this.MaxDateTime.Day; i++)
                {
                    dayList.Add(i);
                }
            }
            else
            {
                for (int i = 1; i <= days; i++)
                {
                    dayList.Add(i);
                }
            }
            this.DayList = new ObservableCollection<int>(dayList);


            var defaultDay = this.DaySelected;
            //首相尝试使用默认值
            if (defaultDay == null)
            {
                if (this.DateTimeSelected.HasValue)
                {
                    defaultDay = this.DateTimeSelected.Value.Day;
                }
            }
            //确保有默认值
            if (defaultDay == null)
            {
                defaultDay = DateTime.Now.Day;
            }

            if (!this.DayList.Contains(defaultDay.Value))
            {
                //这里将日期清空
                this.DateTimeSelected = null;
                defaultDay = this.DayList.FirstOrDefault();
            }

            //主动通知
            this.ForcePropertyChanged(DaySelectedProperty, this.DaySelected, defaultDay);
        }

        [Description("日")]
        public ObservableCollection<int> DayList
        {
            get { return (ObservableCollection<int>)GetValue(DayListProperty); }
            set { SetValue(DayListProperty, value); }
        }

        public static readonly DependencyProperty DayListProperty =
            DependencyProperty.Register("DayList", typeof(ObservableCollection<int>), typeof(RSDateTimePicker), new PropertyMetadata(null));

        [Description("日选择")]
        public int? DaySelected
        {
            get { return (int?)GetValue(DaySelectedProperty); }
            set { SetValue(DaySelectedProperty, value); }
        }

        public static readonly DependencyProperty DaySelectedProperty =
            DependencyProperty.Register("DaySelected", typeof(int?), typeof(RSDateTimePicker), new PropertyMetadata(null, OnDaySelectedPropertyChanged));

        private static void OnDaySelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsDateTimePicker = d as RSDateTimePicker;
            rsDateTimePicker.RefreshHourPicker();
        }

        private void RefreshHourPicker()
        {
            List<int> hourList = new List<int>();
            if (this.YearSelected == this.MinDateTime.Year
                && this.MonthSelected == this.MinDateTime.Month
                && this.DaySelected == this.MinDateTime.Day)
            {
                for (int i = this.MinDateTime.Hour; i < 24; i++)
                {
                    hourList.Add(i);
                }
            }
            else if (this.YearSelected == this.MaxDateTime.Year
                && this.MonthSelected == this.MaxDateTime.Month
                && this.DaySelected == this.MaxDateTime.Day)
            {
                for (int i = 1; i <= this.MaxDateTime.Hour; i++)
                {
                    hourList.Add(i);
                }
            }
            else
            {
                for (int i = 1; i < 24; i++)
                {
                    hourList.Add(i);
                }
            }

            this.HourList = new ObservableCollection<int>(hourList);


            var defaultHour = this.HourSelected;
            //首相尝试使用默认值
            if (defaultHour == null)
            {
                if (this.DateTimeSelected.HasValue)
                {
                    defaultHour = this.DateTimeSelected.Value.Hour;
                }
            }
            //确保有默认值
            if (defaultHour == null)
            {
                defaultHour = DateTime.Now.Hour;
            }

            if (!this.HourList.Contains(defaultHour.Value))
            {
                //这里将日期清空
                this.DateTimeSelected = null;
                defaultHour = this.HourList.FirstOrDefault();
            }

            //主动通知
            this.ForcePropertyChanged(HourSelectedProperty, this.HourSelected, defaultHour);
        }

        [Description("时")]
        public ObservableCollection<int> HourList
        {
            get { return (ObservableCollection<int>)GetValue(HourListProperty); }
            set { SetValue(HourListProperty, value); }
        }

        public static readonly DependencyProperty HourListProperty =
            DependencyProperty.Register("HourList", typeof(ObservableCollection<int>), typeof(RSDateTimePicker), new PropertyMetadata(null));

        [Description("时选择")]
        public int? HourSelected
        {
            get { return (int?)GetValue(HourSelectedProperty); }
            set { SetValue(HourSelectedProperty, value); }
        }

        public static readonly DependencyProperty HourSelectedProperty =
            DependencyProperty.Register("HourSelected", typeof(int?), typeof(RSDateTimePicker), new PropertyMetadata(null, OnHourSelectedPropertyChanged));

        private static void OnHourSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsDateTimePicker = d as RSDateTimePicker;
            rsDateTimePicker.RefreshMinutePicker();
        }

        private void RefreshMinutePicker()
        {
            List<int> minuteList = new List<int>();
            if (this.YearSelected == this.MinDateTime.Year
                && this.MonthSelected == this.MinDateTime.Month
                && this.DaySelected == this.MinDateTime.Day
                && this.HourSelected == this.MinDateTime.Hour)
            {
                for (int i = this.MinDateTime.Hour; i < 60; i++)
                {
                    minuteList.Add(i);
                }
            }
            else if (this.YearSelected == this.MaxDateTime.Year
                && this.MonthSelected == this.MaxDateTime.Month
                && this.DaySelected == this.MaxDateTime.Day
                && this.HourSelected == this.MaxDateTime.Hour)
            {
                for (int i = 1; i <= this.MaxDateTime.Minute; i++)
                {
                    minuteList.Add(i);
                }
            }
            else
            {
                for (int i = 1; i < 60; i++)
                {
                    minuteList.Add(i);
                }
            }

            this.MinuteList = new ObservableCollection<int>(minuteList);

            var defaultMinute = this.MinuteSelected;
            //首相尝试使用默认值
            if (defaultMinute == null)
            {
                if (this.DateTimeSelected.HasValue)
                {
                    defaultMinute = this.DateTimeSelected.Value.Minute;
                }
            }
            //确保有默认值
            if (defaultMinute == null)
            {
                defaultMinute = DateTime.Now.Minute;
            }

            if (!this.MinuteList.Contains(defaultMinute.Value))
            {
                //这里将日期清空
                this.DateTimeSelected = null;
                defaultMinute = this.MinuteList.FirstOrDefault();
            }

            //主动通知
            this.ForcePropertyChanged(MinuteSelectedProperty, this.MinuteSelected, defaultMinute);
        }


        [Description("分")]
        public ObservableCollection<int> MinuteList
        {
            get { return (ObservableCollection<int>)GetValue(MinuteListProperty); }
            set { SetValue(MinuteListProperty, value); }
        }

        public static readonly DependencyProperty MinuteListProperty =
            DependencyProperty.Register("MinuteList", typeof(ObservableCollection<int>), typeof(RSDateTimePicker), new PropertyMetadata(null));

        [Description("分选择")]
        public int? MinuteSelected
        {
            get { return (int?)GetValue(MinuteSelectedProperty); }
            set { SetValue(MinuteSelectedProperty, value); }
        }

        public static readonly DependencyProperty MinuteSelectedProperty =
            DependencyProperty.Register("MinuteSelected", typeof(int?), typeof(RSDateTimePicker), new PropertyMetadata(null, OnMinuteSelectedPropertyChanged));

        private static void OnMinuteSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsDateTimePicker = d as RSDateTimePicker;
            rsDateTimePicker.RefreshSecondPicker();
        }

        private void RefreshSecondPicker()
        {
            List<int> secondList = new List<int>();
            if (this.YearSelected == this.MinDateTime.Year
                && this.MonthSelected == this.MinDateTime.Month
                && this.DaySelected == this.MinDateTime.Day
                && this.HourSelected == this.MinDateTime.Hour
                && this.MinuteSelected == this.MinDateTime.Minute)
            {
                for (int i = this.MinDateTime.Minute; i < 60; i++)
                {
                    secondList.Add(i);
                }
            }
            else if (this.YearSelected == this.MaxDateTime.Year
                && this.MonthSelected == this.MaxDateTime.Month
                && this.DaySelected == this.MaxDateTime.Day
                && this.HourSelected == this.MaxDateTime.Hour
                && this.MinuteSelected == this.MaxDateTime.Minute)
            {
                for (int i = 1; i <= this.MaxDateTime.Minute; i++)
                {
                    secondList.Add(i);
                }
            }
            else
            {
                for (int i = 1; i < 60; i++)
                {
                    secondList.Add(i);
                }
            }

            this.SecondList = new ObservableCollection<int>(secondList);

            var defaultSecond = this.SecondSelected;
            //首相尝试使用默认值
            if (defaultSecond == null)
            {
                if (this.DateTimeSelected.HasValue)
                {
                    defaultSecond = this.DateTimeSelected.Value.Second;
                }
            }
            //确保有默认值
            if (defaultSecond == null)
            {
                defaultSecond = DateTime.Now.Second;
            }

            if (!this.SecondList.Contains(defaultSecond.Value))
            {
                //这里将日期清空
                this.DateTimeSelected = null;
                defaultSecond = this.SecondList.FirstOrDefault();
            }

            //主动通知
            this.ForcePropertyChanged(SecondSelectedProperty, this.SecondSelected, defaultSecond);
        }



        [Description("秒")]
        public ObservableCollection<int> SecondList
        {
            get { return (ObservableCollection<int>)GetValue(SecondListProperty); }
            set { SetValue(SecondListProperty, value); }
        }

        public static readonly DependencyProperty SecondListProperty =
            DependencyProperty.Register("SecondList", typeof(ObservableCollection<int>), typeof(RSDateTimePicker), new PropertyMetadata(null));


        [Description("秒选择")]
        public int? SecondSelected
        {
            get { return (int?)GetValue(SecondSelectedProperty); }
            set { SetValue(SecondSelectedProperty, value); }
        }

        public static readonly DependencyProperty SecondSelectedProperty =
            DependencyProperty.Register("SecondSelected", typeof(int?), typeof(RSDateTimePicker), new PropertyMetadata(null, OnSecondSelectedPropertyChanged));

        private static void OnSecondSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsDateTimePicker = d as RSDateTimePicker;
        }

        [Description("日期选择")]
        public DateTime? DateTimeSelected
        {
            get { return (DateTime?)GetValue(DateTimeSelectedProperty); }
            set { SetValue(DateTimeSelectedProperty, value); }
        }
        public static readonly DependencyProperty DateTimeSelectedProperty =
            DependencyProperty.Register("DateTimeSelected", typeof(DateTime?), typeof(RSDateTimePicker), new PropertyMetadata(null, OnDateTimeSelectedPropertyChanged, OnDateTimeSelectedCoerceValueCallback));

        private static object OnDateTimeSelectedCoerceValueCallback(DependencyObject d, object baseValue)
        {
            var rsDateTimePicker = d as RSDateTimePicker;
            var dateTime = baseValue as DateTime?;
            if (dateTime.HasValue)
            {
                if (dateTime.Value < rsDateTimePicker.MinDateTime
                    || dateTime.Value > rsDateTimePicker.MaxDateTime)
                {
                    IWindow window = rsDateTimePicker.TryFindParent<RSWindow>();
                    window?.ShowWarningInfoAsync("数据范围越界");
                    return null;
                }
            }
            return baseValue;
        }

        private static void OnDateTimeSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsDateTimePicker = d as RSDateTimePicker;
            rsDateTimePicker.ChcekYearMonthDayHourMinuteSecondSelect();
            rsDateTimePicker.UpdateFormattedDateTime();
        }

        private void ChcekYearMonthDayHourMinuteSecondSelect()
        {
            //这里检查一下 
            if (this.DateTimeSelected.HasValue)
            {
                var dateTime = this.DateTimeSelected.Value;
                if (dateTime.Year != this.YearSelected)
                {
                    this.YearSelected = dateTime.Year;
                }
                if (dateTime.Month != this.MonthSelected)
                {
                    this.MonthSelected = dateTime.Month;
                }
                if (dateTime.Day != this.DaySelected)
                {
                    this.DaySelected = dateTime.Day;
                }
                if (dateTime.Hour != this.HourSelected)
                {
                    this.HourSelected = dateTime.Hour;
                }
                if (dateTime.Minute != this.MinuteSelected)
                {
                    this.MinuteSelected = dateTime.Minute;
                }
                if (dateTime.Second != this.SecondSelected)
                {
                    this.SecondSelected = dateTime.Second;
                }
            }
        }

        [Description("日期格式化")]
        public string DateTimeFormat
        {
            get { return (string)GetValue(DateTimeFormatProperty); }
            set { SetValue(DateTimeFormatProperty, value); }
        }

        public static readonly DependencyProperty DateTimeFormatProperty =
            DependencyProperty.Register("DateTimeFormat", typeof(string), typeof(RSDateTimePicker), new PropertyMetadata(null, OnDateTimeFormatPropertyChanged));

        private static void OnDateTimeFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsDateTimePicker = d as RSDateTimePicker;
            rsDateTimePicker.UpdateFormattedDateTime();
        }


        [Description("格式化后的文本")]
        public string FormattedDateTime
        {
            get { return (string)GetValue(FormattedDateTimeProperty); }
            set { SetValue(FormattedDateTimeProperty, value); }
        }

        public static readonly DependencyProperty FormattedDateTimeProperty =
            DependencyProperty.Register("FormattedDateTime", typeof(string), typeof(RSDateTimePicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFormattedDateTimeChanged));


        private static void OnFormattedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsDateTimePicker = d as RSDateTimePicker;
            if (rsDateTimePicker.IsCanUpdateDateTimeSelected)
            {
                rsDateTimePicker.IsCanUpdateFormattedDateTime = false;
                try
                {
                    if (!string.IsNullOrEmpty(rsDateTimePicker.FormattedDateTime))
                    {
                        if (DateTime.TryParse(rsDateTimePicker.FormattedDateTime, out DateTime dt))
                        {
                            rsDateTimePicker.DateTimeSelected = dt;
                        }
                        else
                        {
                            throw new Exception("日期格式不正确");
                        }
                    }
                    else
                    {
                        rsDateTimePicker.DateTimeSelected = null;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    rsDateTimePicker.IsCanUpdateFormattedDateTime = true;
                }
            }
        }


        [Description("是否可以搜索")]
        public bool IsCanSearch
        {
            get { return (bool)GetValue(IsCanSearchProperty); }
            set { SetValue(IsCanSearchProperty, value); }
        }

        public static readonly DependencyProperty IsCanSearchProperty =
            DependencyProperty.Register("IsCanSearch", typeof(bool), typeof(RSDateTimePicker), new PropertyMetadata(true));



        [Description("日期格式")]
        public DateTimeParts DisplayParts
        {
            get { return (DateTimeParts)GetValue(DisplayPartsProperty); }
            set { SetValue(DisplayPartsProperty, value); }
        }

        public static readonly DependencyProperty DisplayPartsProperty =
            DependencyProperty.Register("DisplayParts", typeof(DateTimeParts), typeof(RSDateTimePicker), new PropertyMetadata(DateTimeParts.None));



        [Description("分隔符配置")]

        public string DateSeparator
        {
            get { return (string)GetValue(DateSeparatorProperty); }
            set { SetValue(DateSeparatorProperty, value); }
        }

        public static readonly DependencyProperty DateSeparatorProperty =
            DependencyProperty.Register("DateSeparator", typeof(string), typeof(RSDateTimePicker), new PropertyMetadata("-"));



        [Description("分隔符配置")]
        public string TimeSeparator
        {
            get { return (string)GetValue(TimeSeparatorProperty); }
            set { SetValue(TimeSeparatorProperty, value); }
        }

        public static readonly DependencyProperty TimeSeparatorProperty =
            DependencyProperty.Register("TimeSeparator", typeof(string), typeof(RSDateTimePicker), new PropertyMetadata(":"));



        [Description("分隔符配置")]
        public string DateTimeSeparator
        {
            get { return (string)GetValue(DateTimeSeparatorProperty); }
            set { SetValue(DateTimeSeparatorProperty, value); }
        }

        public static readonly DependencyProperty DateTimeSeparatorProperty =
            DependencyProperty.Register("DateTimeSeparator", typeof(string), typeof(RSDateTimePicker), new PropertyMetadata(" "));


        [Description("是否只读")]
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RSDateTimePicker), new PropertyMetadata(true));




        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Popup = this.GetTemplateChild(nameof(this.PART_Popup)) as RSPopup;
            this.PART_BtnDatePicker = this.GetTemplateChild(nameof(this.PART_BtnDatePicker)) as ToggleButton;
            this.PART_Border = this.GetTemplateChild(nameof(this.PART_Border)) as Border;
            this.PART_PopupHost = this.GetTemplateChild(nameof(this.PART_PopupHost)) as Grid;
            this.PART_BtnConfirm = this.GetTemplateChild(nameof(this.PART_BtnConfirm)) as Button;
            this.PART_BtnCancel = this.GetTemplateChild(nameof(this.PART_BtnCancel)) as Button;

            if (this.PART_Popup != null)
            {
                this.PART_Popup.Opened += PART_Popup_Opened;
            }

            if (this.PART_BtnConfirm != null)
            {
                this.PART_BtnConfirm.Click += PART_BtnConfirm_Click;
            }

            if (this.PART_BtnCancel != null)
            {
                this.PART_BtnCancel.Click += PART_BtnCancel_Click;
            }
        }

        private void PART_BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.HiddenPopup();
        }

        private void PART_BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateDateTimeSelect();
            this.HiddenPopup();
        }

        private void HiddenPopup()
        {
            this.PART_Popup.IsOpen = false;
        }

        private void RefreshDefaultDateTimeSelect()
        {
            var dateTimeDefault = DateTime.Now;
            if (this.DateTimeSelected.HasValue)
            {
                dateTimeDefault = this.DateTimeSelected.Value;
            }
            this.YearSelected = dateTimeDefault.Year;
            this.MonthSelected = dateTimeDefault.Month;
            this.DaySelected = dateTimeDefault.Day;
            this.HourSelected = dateTimeDefault.Hour;
            this.MinuteSelected = dateTimeDefault.Minute;
            this.SecondSelected = dateTimeDefault.Second;
        }

        private void PART_Popup_Opened(object? sender, EventArgs e)
        {
            this.RefreshDefaultDateTimeSelect();
            this.UpdatePopupSize();
        }

        private void UpdatePopupSize()
        {
            var actualWidth = this.PART_Border.ActualWidth;
            var actualHeight = this.PART_Border.ActualHeight;
            var popupActualWidth = this.PART_PopupHost.ActualWidth;
            var popupActualHeight = this.PART_PopupHost.ActualHeight;
            this.PART_Popup.VerticalOffset = -(popupActualHeight / 2 + actualHeight / 2);
        }


        private void UpdateDateTimeSelect()
        {
            if (this.YearSelected == null
                || this.MonthSelected == null
                || this.DaySelected == null
                || this.HourSelected == null
                || this.MinuteSelected == null
                || this.SecondSelected == null)
            {
                this.DateTimeSelected = null;
                return;
            }
            this.DateTimeSelected = new DateTime(this.YearSelected.Value,
                         this.MonthSelected.Value,
                         this.DaySelected.Value,
                         this.HourSelected.Value,
                         this.MinuteSelected.Value,
                         this.SecondSelected.Value);
        }

        private void UpdateFormattedDateTime()
        {
            if (this.IsCanUpdateFormattedDateTime)
            {
                this.IsCanUpdateDateTimeSelected = false;
                if (this.DateTimeSelected.HasValue)
                {
                    this.FormattedDateTime = this.FormatDateTime(this.DateTimeSelected.Value);
                }
                else
                {
                    this.FormattedDateTime = null;
                }
                this.IsCanUpdateDateTimeSelected = true;
            }
        }


        // 根据所选部分格式化日期时间
        public string FormatDateTime(DateTime dateTime)
        {

            string format = "";

            // 年
            if ((DisplayParts & DateTimeParts.Year) != 0)
            {
                format += "yyyy";
            }

            // 月
            if ((DisplayParts & DateTimeParts.Month) != 0)
            {
                if (!string.IsNullOrEmpty(format)) format += DateSeparator;
                format += "MM";
            }

            // 日
            if ((DisplayParts & DateTimeParts.Day) != 0)
            {
                if (!string.IsNullOrEmpty(format)) format += DateSeparator;
                format += "dd";
            }

            // 如果同时包含日期和时间部分，添加分隔符
            if ((DisplayParts & DateTimeParts.Date) != 0 &&
                (DisplayParts & DateTimeParts.Time) != 0)
            {
                format += DateTimeSeparator;
            }

            // 时
            if ((DisplayParts & DateTimeParts.Hour) != 0)
            {
                format += "HH";
            }

            // 分
            if ((DisplayParts & DateTimeParts.Minute) != 0)
            {
                if (!string.IsNullOrEmpty(format)) format += TimeSeparator;
                format += "mm";
            }

            // 秒
            if ((DisplayParts & DateTimeParts.Second) != 0)
            {
                if (!string.IsNullOrEmpty(format)) format += TimeSeparator;
                format += "ss";
            }

            // 如果没有选择任何部分，返回默认格式
            if (string.IsNullOrEmpty(format))
            {
                if (!string.IsNullOrEmpty(this.DateTimeFormat))
                {
                    return dateTime.ToString(this.DateTimeFormat);
                }
                else
                {
                    format = "yyyy-MM-dd HH:mm:ss";
                }
            }

            return dateTime.ToString(format);
        }



        /// <summary>
        /// 强制触发属性变化通知
        /// </summary>
        /// <param name="dependencyProperty">依赖属性</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        public  void ForcePropertyChanged(DependencyProperty dependencyProperty, object oldValue, object newValue)
        {
            if (oldValue == null || !oldValue.Equals(newValue))
            {
                this.SetValue(dependencyProperty, newValue);
            }
            else
            {
                //只有相同才强制刷新
                var metadata = dependencyProperty.GetMetadata(this);
                metadata.PropertyChangedCallback(this,
                    new DependencyPropertyChangedEventArgs(dependencyProperty, oldValue, newValue));
            }
        }
    }
}
