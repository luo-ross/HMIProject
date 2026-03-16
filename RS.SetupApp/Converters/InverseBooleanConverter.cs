using System.Globalization;
using System.Windows.Data;

namespace RS.SetupApp.Converters;

public sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : System.Windows.Data.Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : System.Windows.Data.Binding.DoNothing;
    }
}
