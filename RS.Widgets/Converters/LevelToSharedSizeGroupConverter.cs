using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    public class LevelToSharedSizeGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            int level = 0;
            if (value is int)
            {
                level = (int)value;
            }
            else
            {
                int.TryParse(value.ToString(), out level);
            }

            string prefix = null;
            if (parameter != null)
            {
                prefix = parameter.ToString();
            }
            else
            {
                prefix = "Level";
            }

            return prefix + level.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
