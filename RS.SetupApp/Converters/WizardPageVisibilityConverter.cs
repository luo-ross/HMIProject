using System.Globalization;
using System.Windows;
using System.Windows.Data;
using RS.SetupApp.ViewModels;

namespace RS.SetupApp.Converters;

public sealed class WizardPageVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is WizardPageKind currentPage &&
            parameter is string pageName &&
            Enum.TryParse<WizardPageKind>(pageName, ignoreCase: true, out WizardPageKind expectedPage))
        {
            return currentPage == expectedPage ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
