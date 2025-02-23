using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Converters
{
    internal class SystemWorkAreaToMarginConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(
                SystemParameters.WorkArea.Left
              , SystemParameters.WorkArea.Top
              , SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Width
              , SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height
              );
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
