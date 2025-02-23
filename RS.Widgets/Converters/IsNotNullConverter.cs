using System;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public sealed class IsNotNullConverter : IValueConverter
    {
        /// <summary>
        /// Gets a static default instance of <see cref="IsNotNullConverter"/>.
        /// </summary>
        public static readonly IsNotNullConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return value is not null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}