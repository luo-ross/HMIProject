using RS.Widgets.Models;
using System;
using System.Windows;

namespace RS.Widgets
{
    /// <summary>
    /// 日历日期选中事件参数
    /// </summary>
    public class CalendarDateSelectedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 选中的日期
        /// </summary>
        public DateTime DateSelected { get; }

        /// <summary>
        /// 选中的日历项模型
        /// </summary>
        public CalendarBaseModel CalendarItem { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="selectedDate">选中的日期</param>
        /// <param name="calendarItem">选中的日历项模型</param>
        public CalendarDateSelectedEventArgs(RoutedEvent routedEvent, DateTime dateSelected, CalendarBaseModel calendarItem)
            : base(routedEvent)
        {
            DateSelected = dateSelected;
            CalendarItem = calendarItem;
        }
    }
}

