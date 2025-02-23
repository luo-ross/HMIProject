using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    public class ByteSizeConverter : IValueConverter
    {
        // 转换方法（字节 -> 易读单位）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long bytes)
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" }; // 单位列表
                int order = 0; // 单位索引
                // 计算合适的单位
                while (bytes >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    bytes /= 1024;
                }
                // 返回格式化后的字符串
                return $"{bytes:0.##} {sizes[order]}";
            }

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
