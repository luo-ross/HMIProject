using CommunityToolkit.Mvvm.Input;
using RS.Widgets.Enums;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
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
    public class RSCalendarDatePicker : RSDatePickerBase
    {
        private RSCalendar PART_Calendar;
        static RSCalendarDatePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSCalendarDatePicker), new FrameworkPropertyMetadata(typeof(RSCalendarDatePicker)));
            DateTimeSelectedProperty.OverrideMetadata(typeof(RSCalendarDatePicker), new FrameworkPropertyMetadata(null, OnDateTimeSelectedPropertyChanged, OnDateTimeSelectedCoerceValueCallback));
        }

        private static void OnDateTimeSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var calendarDatePicker = d as RSCalendarDatePicker;
            calendarDatePicker?.UpdateFormattedDateTime();
        }


        public RSCalendarDatePicker()
        {

        }


        /// <summary>
        /// 日历类型
        /// </summary>
        public CalendarSelectType CalendarSelectType
        {
            get { return (CalendarSelectType)GetValue(CalendarSelectTypeProperty); }
            set { SetValue(CalendarSelectTypeProperty, value); }
        }

        public static readonly DependencyProperty CalendarSelectTypeProperty =
            DependencyProperty.Register(nameof(CalendarSelectType), typeof(CalendarSelectType), typeof(RSCalendarDatePicker), new PropertyMetadata(CalendarSelectType.Day, OnCalendarSelectTypePropertyChanged));

        private static void OnCalendarSelectTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var calendarDatePicker = d as RSCalendarDatePicker;
           
            calendarDatePicker?.UpdateDateTimeFormat();
        }

        private void UpdateDateTimeFormat()
        {
            switch (this.CalendarSelectType)
            {
                case CalendarSelectType.Day:
                    this.SetCurrentValue(RSDatePicker.DateTimeFormatProperty,"yyyy-MM-dd");
                    break;
                case CalendarSelectType.Month:
                    this.SetCurrentValue(RSDatePicker.DateTimeFormatProperty, "yyyy-MM");
                    break;
                case CalendarSelectType.Year:
                    this.SetCurrentValue(RSDatePicker.DateTimeFormatProperty, "yyyy");
                    break;
            }
            this?.UpdateFormattedDateTime();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Calendar = this.GetTemplateChild(nameof(this.PART_Calendar)) as RSCalendar;
            this.PART_Popup = this.GetTemplateChild(nameof(this.PART_Popup)) as RSPopup;
            if (this.PART_Calendar!=null)
            {
                this.PART_Calendar.DateSelected += PART_Calendar_DateSelected;
            }


            if (this.PART_Popup != null)
            {
                this.PART_Popup.Opened += PART_Popup_Opened;
            }

        }

        private void PART_Popup_Opened(object? sender, EventArgs e)
        {
            this.PART_Calendar.DateTimeSelected = this.DateTimeSelected;
        }

        private void PART_Calendar_DateSelected(object? sender, CalendarDateSelectedEventArgs e)
        {
            this.DateTimeSelected = e.DateSelected;
            this.HiddenPopup();
        }
       
    }
}
