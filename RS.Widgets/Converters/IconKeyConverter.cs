using RS.Widgets.Controls;
using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    public class IconKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 检查传入的值是否是我们期望的 IconKey 类型
            if (value is IconKey key)
            {
                // 使用我们之前创建的 IconProvider 来获取 Geometry
                return IconHelper.GetGeometry(key);
            }
            return null;
        }

        // ConvertBack 通常用于双向绑定，这里我们不需要，所以返回 null
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
