using System;
using System.Globalization;
using System.Windows.Data;

namespace RS.Widgets.Converters
{

    public class MathTruncateConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            var value1 = (value as double?).GetValueOrDefault(System.Convert.ToDouble(value, CultureInfo.InvariantCulture));
            return Math.Truncate(value1);
        }



        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
