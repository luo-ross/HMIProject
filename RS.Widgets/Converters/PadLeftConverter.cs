using RS.Commons.Extend;
using System;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    public sealed class PadLeftConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return value;
            }
            var totalWidth = parameter.ToInt();
            return value.ToString().PadLeft(totalWidth, '0');
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}