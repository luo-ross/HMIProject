using System;
using System.Globalization;
using System.Windows.Data;

namespace RS.WPFClient.Converters
{
    /// <summary>
    /// 邮件时间格式化转换器，根据日期显示不同格式
    /// </summary>
    public class EmailTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is DateTime dateTime))
            {
                return string.Empty;
            }

            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);

            if (dateTime.Date == today)
            {
                // 今天显示时间，如 "07:44"
                return dateTime.ToString("HH:mm");
            }
            else if (dateTime.Date == yesterday)
            {
                // 昨天显示 "昨天 HH:mm"
                return $"昨天 {dateTime:HH:mm}";
            }
            else if (dateTime.Date >= thisWeekStart)
            {
                // 本周显示 "MM月DD日"
                return dateTime.ToString("M月d日");
            }
            else
            {
                // 更早显示 "yyyy/MM/dd"
                return dateTime.ToString("yyyy/MM/dd");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}




