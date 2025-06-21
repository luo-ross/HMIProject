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
        
        public static readonly DependencyProperty CheckedIconFillProperty =
            DependencyProperty.RegisterAttached(
                "CheckedIconFill",
                typeof(Brush),
                typeof(MenuItemHelper),
                new PropertyMetadata(null));
        public static Brush GetCheckedIconFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(CheckedIconFillProperty);
        }

        public static void SetImageData(DependencyObject obj, Brush value)
        {
            obj.SetValue(CheckedIconFillProperty, value);
        }



        /// <summary>
        /// 选中图像样式
        /// </summary>
        public static readonly DependencyProperty CheckedIconProperty =
            DependencyProperty.RegisterAttached(
                "CheckedIcon",
                typeof(Geometry),
                typeof(MenuItemHelper),
                new PropertyMetadata(null));
        public static Geometry GetCheckedIcon(DependencyObject obj)
        {
            return (Geometry)obj.GetValue(CheckedIconProperty);
        }

        public static void SetCheckedIcon(DependencyObject obj, Geometry value)
        {
            obj.SetValue(CheckedIconProperty, value);
        }

    }
}
