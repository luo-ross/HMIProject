using RS.Widgets.Enums;
using RS.Widgets.Interfaces;
using RS.Widgets.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RS.Widgets.Controls
{
    public abstract class RSDatePickerBase : ContentControl
    {
        protected RSPopup PART_Popup;
        protected Grid PART_PopupHost;
        protected bool IsCanUpdateDateTimeSelected = true;
        protected bool IsCanUpdateFormattedDateTime = true;

        public DateTime MinDate
        {
            get { return (DateTime)GetValue(MinDateProperty); }
            set { SetValue(MinDateProperty, value); }
        }

        public static readonly DependencyProperty MinDateProperty =
            DependencyProperty.Register("MinDate", typeof(DateTime), typeof(RSDatePickerBase), new PropertyMetadata(DateTime.MinValue));

      

        public DateTime MaxDate
        {
            get { return (DateTime)GetValue(MaxDateProperty); }
            set { SetValue(MaxDateProperty, value); }
        }

        public static readonly DependencyProperty MaxDateProperty =
            DependencyProperty.Register("MaxDate", typeof(DateTime), typeof(RSDatePickerBase), new PropertyMetadata(DateTime.MaxValue));
      


        [Description("圆角大小")]
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(RSDatePickerBase), new PropertyMetadata(new CornerRadius(5)));



        [Description("日期选择")]
        public DateTime? DateTimeSelected
        {
            get { return (DateTime?)GetValue(DateTimeSelectedProperty); }
            set { SetValue(DateTimeSelectedProperty, value); }
        }
        public static readonly DependencyProperty DateTimeSelectedProperty =
            DependencyProperty.Register("DateTimeSelected", typeof(DateTime?), typeof(RSDatePickerBase), new PropertyMetadata(null));

        protected static object OnDateTimeSelectedCoerceValueCallback(DependencyObject d, object baseValue)
        {
            var datePicker = d as RSDatePickerBase;
            var dateTime = baseValue as DateTime?;
            if (dateTime.HasValue)
            {
                if (dateTime.Value < datePicker.MinDate
                    || dateTime.Value > datePicker.MaxDate)
                {
                    IWindow window = datePicker.TryFindParent<RSWindow>();
                    window?.ShowWarningInfoAsync("Data out of bound");
                    return null;
                }
            }
            return baseValue;
        }
     

        [Description("日期格式化")]
        public string DateTimeFormat
        {
            get { return (string)GetValue(DateTimeFormatProperty); }
            set { SetValue(DateTimeFormatProperty, value); }
        }

        public static readonly DependencyProperty DateTimeFormatProperty =
            DependencyProperty.Register("DateTimeFormat", typeof(string), typeof(RSDatePickerBase), new PropertyMetadata(null, OnDateTimeFormatPropertyChanged));

        private static void OnDateTimeFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var datePicker = d as RSDatePickerBase;
            datePicker.UpdateFormattedDateTime();
        }


        [Description("格式化后的文本")]
        public string FormattedDateTime
        {
            get { return (string)GetValue(FormattedDateTimeProperty); }
            set { SetValue(FormattedDateTimeProperty, value); }
        }

        public static readonly DependencyProperty FormattedDateTimeProperty =
            DependencyProperty.Register("FormattedDateTime", typeof(string), typeof(RSDatePickerBase),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFormattedDateTimeChanged));


        private static void OnFormattedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var datePicker = d as RSDatePickerBase;
            if (datePicker.IsCanUpdateDateTimeSelected)
            {
                datePicker.IsCanUpdateFormattedDateTime = false;
                try
                {
                    if (!string.IsNullOrEmpty(datePicker.FormattedDateTime))
                    {
                        if (DateTime.TryParse(datePicker.FormattedDateTime, out DateTime dt))
                        {
                            datePicker.DateTimeSelected = dt;
                        }
                        else
                        {
                            throw new Exception("日期格式不正确");
                        }
                    }
                    else
                    {
                        datePicker.DateTimeSelected = null;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    datePicker.IsCanUpdateFormattedDateTime = true;
                }
            }
        }


        [Description("日期格式")]
        public DateTimeParts DisplayParts
        {
            get { return (DateTimeParts)GetValue(DisplayPartsProperty); }
            set { SetValue(DisplayPartsProperty, value); }
        }

        public static readonly DependencyProperty DisplayPartsProperty =
            DependencyProperty.Register("DisplayParts", typeof(DateTimeParts), typeof(RSDatePickerBase), new PropertyMetadata(DateTimeParts.None));



        [Description("分隔符配置")]
        public string DateSeparator
        {
            get { return (string)GetValue(DateSeparatorProperty); }
            set { SetValue(DateSeparatorProperty, value); }
        }

        public static readonly DependencyProperty DateSeparatorProperty =
            DependencyProperty.Register("DateSeparator", typeof(string), typeof(RSDatePickerBase), new PropertyMetadata("-"));



        [Description("分隔符配置")]
        public string TimeSeparator
        {
            get { return (string)GetValue(TimeSeparatorProperty); }
            set { SetValue(TimeSeparatorProperty, value); }
        }

        public static readonly DependencyProperty TimeSeparatorProperty =
            DependencyProperty.Register("TimeSeparator", typeof(string), typeof(RSDatePickerBase), new PropertyMetadata(":"));



        [Description("分隔符配置")]
        public string DateTimeSeparator
        {
            get { return (string)GetValue(DateTimeSeparatorProperty); }
            set { SetValue(DateTimeSeparatorProperty, value); }
        }

        public static readonly DependencyProperty DateTimeSeparatorProperty =
            DependencyProperty.Register("DateTimeSeparator", typeof(string), typeof(RSDatePickerBase), new PropertyMetadata(" "));


        [Description("是否只读")]
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RSDatePickerBase), new PropertyMetadata(true));


        protected void HiddenPopup()
        {
            this.PART_Popup.SetCurrentValue(Popup.IsOpenProperty, false);
        }

        protected void UpdateFormattedDateTime()
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
        protected string FormatDateTime(DateTime dateTime)
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
        protected void ForcePropertyChanged(DependencyProperty dependencyProperty, object oldValue, object newValue)
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
