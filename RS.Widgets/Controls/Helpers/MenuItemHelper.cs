using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace RS.Widgets.Controls
{
    public class MenuItemHelper
    {
        /// <summary>
        /// Select Icon Fill
        /// </summary>
        public static readonly DependencyProperty IconFillProperty =
            DependencyProperty.RegisterAttached(
                "IconFill",
                typeof(Brush),
                typeof(MenuItemHelper),
                new PropertyMetadata(null));
        public static Brush GetIconFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(IconFillProperty);
        }

        public static void SetImageData(DependencyObject obj, Brush value)
        {
            obj.SetValue(IconFillProperty, value);
        }



        /// <summary>
        /// 这是自定义Pata 路径
        /// </summary>
        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.RegisterAttached(
                "IconData",
                typeof(Geometry),
                typeof(MenuItemHelper),
                new PropertyMetadata(null));
        public static Geometry GetIconData(DependencyObject obj)
        {
            return (Geometry)obj.GetValue(IconDataProperty);
        }

        public static void SetIconData(DependencyObject obj, Geometry value)
        {
            obj.SetValue(IconDataProperty, value);
        }

    }
}
