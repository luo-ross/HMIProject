using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace RS.Widgets.Converters
{
    public class NavIndentationConverter : IValueConverter
    {
        // ConverterParameter 传递缩进基数
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new Thickness(0);
            }

            int level = 0;
            int basePadding = 15; // 默认基数

            // 解析level
            if (value is int)
            {
                level = (int)value;
            }
            else
            {
                int.TryParse(value.ToString(), out level);
            }

            // 解析参数
            if (parameter != null)
            {
                int.TryParse(parameter.ToString(), out basePadding);
            }
            double left = level * basePadding;
            return new Thickness(left, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
