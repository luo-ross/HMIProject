using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    public class CanvasClipRectConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
            {
                return new Rect(0, 0, 0, 0);
            }
            double width = 0D;
            double height = 0D;
            if (values[0] != DependencyProperty.UnsetValue)
            {
                 width = System.Convert.ToDouble(values[0]);
            }
            if (values[1] != DependencyProperty.UnsetValue)
            {
                 height = System.Convert.ToDouble(values[1]);
            }
        
            return new Rect(0D, 0D, width, height);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
