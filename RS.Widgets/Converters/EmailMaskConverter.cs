using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    public class EmailMaskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string email))
            {
                return value;
            }

            // 检查是否包含@符号
            int atIndex = email.IndexOf('@');
            if (atIndex <= 1) // 如果@在第一位或之前，直接返回原值
            {
                atIndex = email.Length - 1;
            }

            // 获取@前的字符串
            string prefix = email.Substring(0, atIndex);

            // 获取@后的字符串
            string suffix = email.Substring(atIndex);

            if (prefix.Length == 1)
            {
                var maskedPrefix = prefix.PadLeft(Math.Max(prefix.Length, 8), '*');
                return $"{maskedPrefix}{suffix}";
            }
            else
            {
                var maskedPrefix = prefix[prefix.Length - 1].ToString().PadLeft(Math.Max(prefix.Length - 1, 8), '*');
                return $"{prefix[0]}{maskedPrefix}{suffix}";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
