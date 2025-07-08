using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Commons
{
    public static class VisibilityBoxes
    {
        public static object VisibleBox = Visibility.Visible;

        public static object HiddenBox = Visibility.Hidden;

        public static object CollapsedBox = Visibility.Collapsed;

        public static object Box(Visibility value)
        {
            return value switch
            {
                Visibility.Visible => VisibleBox,
                Visibility.Hidden => HiddenBox,
                _ => CollapsedBox,
            };
        }
    }
}
