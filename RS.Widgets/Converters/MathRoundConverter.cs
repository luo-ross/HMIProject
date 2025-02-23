using System;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    public sealed class MathRoundConverter : IValueConverter
    {

        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            int digits = 2;
            if (parameter != null&&int.TryParse(parameter.ToString(),out int digitsOut))
            {
                digits= digitsOut ;
            }
            

            if (double.TryParse(value.ToString(), out double intputValue))
            {
                return Math.Round(intputValue, digits);
            }
            else
            {
                return value;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}