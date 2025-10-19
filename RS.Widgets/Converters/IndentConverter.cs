using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace RS.Widgets.Converters
{
    public class IndentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double paddingLeft = 10;
            double left = 0.0;
            UIElement treeViewItem = value as TreeViewItem;
            while (treeViewItem.GetType() != typeof(TreeView))
            {
                treeViewItem = (UIElement)VisualTreeHelper.GetParent(treeViewItem);
                if (treeViewItem.GetType() == typeof(TreeViewItem)) { 
                    left += paddingLeft;
                }
            }
            return new Thickness(left, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
