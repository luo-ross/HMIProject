using RS.Widgets.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RS.Widgets.Converters
{
    public class NavChildrenFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var items = value as List<NavigateModel>;
            string parentId = parameter as string;
            if (items == null ||  string.IsNullOrEmpty(parentId)) {
                return null;
            }
            var result = items.Where(x => x.ParentId == parentId).ToList();
            return new ObservableCollection<NavigateModel>(result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;   
        }
    }
}
