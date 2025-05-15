using RS.Commons.Extend;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    public class HowManyRowsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 3)
            {
                var page = values[0].ToInt();
                var rows = values[1].ToInt();
                var records = values[2].ToInt();
                return $"{Math.Min(page * rows, records)}";
            }
            return $"{0}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
