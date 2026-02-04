using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RS.WPFClient.Converters
{
    /// <summary>
    /// Adaptive layout converter for InBoxView.
    /// Values: [0] IsPreviewMode (bool), [1] ActualWidth (double)
    /// Parameter: "ShowPreviewPane" or "UseCompactTemplate"
    /// </summary>
    public class AdaptiveLayoutConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                return false;
            }
            
            if (!(values[0] is bool isPreviewMode))
            {
                return false;
            }
            
            if (!(values[1] is double totalWidth))
            {
                return false;
            }

            if (!(values[2] is double containerWidth))
            {
                return false;
            }

            string mode = parameter as string;

            if (mode == "ShowPreviewPane")
            {
                // Effective preview mode: toggle is ON and TOTAL window width is sufficient (> 825)
                return isPreviewMode && totalWidth > 825;
            }

            if (mode == "UseCompactTemplate")
            {
                // Compact template: used whenever the specific LIST container width is small (< 745)
                // This correctly handles both small windows AND large windows in preview mode
                return containerWidth < 745;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
