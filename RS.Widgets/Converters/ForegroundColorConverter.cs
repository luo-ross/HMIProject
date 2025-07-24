using RS.Widgets.Commons;
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace RS.Widgets.Converters
{

    public class ForegroundColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Colors.Black.ToString(); ;

            Color backgroundColor = Colors.White;
            if (value is string strColor)
            {
                backgroundColor = (Color)ColorConverter.ConvertFromString(strColor);
            }
            else if (value is Color color)
            {
                backgroundColor = color;
            }

            int luminance = ColorHelper.CalculateLuminance(backgroundColor);
            // 假设亮度阈值为128  
            if (luminance > 128)
            {
                // 背景较亮，使用黑色文字  
                return Colors.Black.ToString();
            }
            else
            {
                // 背景较暗，使用白色文字  
                return Colors.White.ToString();
            }
        }



        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
