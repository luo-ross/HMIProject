using RS.Widgets.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RS.Widgets.Converters
{

    public class IndexGetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            var values0 = values[0];
            var values1 = values[1];
            if (values0 == DependencyProperty.UnsetValue || values1 == DependencyProperty.UnsetValue)
            {
                return 0;
            }

            if (values0 == null || values1 == null)
            {
                return 0;
            }


            var valueList = values[1] as IList;
            if (valueList == null)
            {
                return 0;
            }

            if (valueList.Contains(values0))
            {
                var returnIndex = valueList.IndexOf(values0) + 1;
                return returnIndex;
            }
            else
            {
                return 0;
            }


        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
