using System;
using System.Globalization;
using System.Windows.Data;

namespace RS.WPFClient.Converters
{
    /// <summary>
    /// 邮件日期分组转换器，将日期转换为"今天"、"周二"、"更早"等格式
    /// </summary>
    public class EmailDateGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is DateTime dateTime))
            {
                return "更早";
            }

            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);

            if (dateTime.Date == today)
            {
                return "今天";
            }
            else if (dateTime.Date == yesterday)
            {
                return "昨天";
            }
            else if (dateTime.Date >= thisWeekStart)
            {
                // 本周的邮件，显示星期几
                var dayOfWeek = dateTime.DayOfWeek;
                return GetDayOfWeekName(dayOfWeek);
            }
            else
            {
                return "更早";
            }
        }

        private string GetDayOfWeekName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "周一",
                DayOfWeek.Tuesday => "周二",
                DayOfWeek.Wednesday => "周三",
                DayOfWeek.Thursday => "周四",
                DayOfWeek.Friday => "周五",
                DayOfWeek.Saturday => "周六",
                DayOfWeek.Sunday => "周日",
                _ => "更早"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}




