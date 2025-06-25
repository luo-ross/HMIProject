using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public static class ControlsHelper
    {
        /// <summary>
        /// 图像资源
        /// </summary>
        public static readonly DependencyProperty ImageDataProperty =
            DependencyProperty.RegisterAttached(
                "ImageData",
                typeof(ImageSource),
                typeof(ControlsHelper),
                new PropertyMetadata(null));
        public static ImageSource GetImageData(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImageDataProperty);
        }

        public static void SetImageData(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImageDataProperty, value);
        }



        /// <summary>
        /// 这是自定义Pata 路径
        /// </summary>
        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.RegisterAttached(
                "IconData",
                typeof(Geometry),
                typeof(ControlsHelper),
                new PropertyMetadata(null));
        public static Geometry GetIconData(DependencyObject obj)
        {
            return (Geometry)obj.GetValue(IconDataProperty);
        }

        public static void SetIconData(DependencyObject obj, Geometry value)
        {
            obj.SetValue(IconDataProperty, value);
        }


        /// <summary>
        /// 这是Icon宽度
        /// </summary>
        
        public static readonly DependencyProperty IconWidthProperty =
          DependencyProperty.RegisterAttached(
              "IconWidth",
              typeof(double),
              typeof(ControlsHelper),
              new PropertyMetadata(12D));

        public static double GetIconWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(IconWidthProperty);
        }

        public static void SetIconWidth(DependencyObject obj, double value)
        {
            obj.SetValue(IconWidthProperty, value);
        }


        /// <summary>
        /// 这是Icon高度
        /// </summary>
        public static readonly DependencyProperty IconHeightProperty =
          DependencyProperty.RegisterAttached(
              "IconHeight",
              typeof(double),
              typeof(ControlsHelper),
              new PropertyMetadata(12D));

        public static double GetIconHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(IconHeightProperty);
        }

        public static void SetIconHeight(DependencyObject obj, double value)
        {
            obj.SetValue(IconHeightProperty, value);
        }


        /// <summary>
        /// 设置控件圆角
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
          DependencyProperty.RegisterAttached(
              "CornerRadius",
              typeof(CornerRadius),
              typeof(ControlsHelper),
              new PropertyMetadata(default));

        public static CornerRadius GetCornerRadius(DependencyObject obj)
        {
            return (CornerRadius)obj.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }



        /// <summary>
        /// 空数据描述
        /// </summary>
        public static readonly DependencyProperty EmptyDataDesProperty =
          DependencyProperty.RegisterAttached(
              "EmptyDataDes",
              typeof(string),
              typeof(ControlsHelper),
              new PropertyMetadata("空空如也..."));

        public static string GetEmptyDataDes(DependencyObject obj)
        {
            return (string)obj.GetValue(EmptyDataDesProperty);
        }

        public static void SetEmptyDataDes(DependencyObject obj, string value)
        {
            obj.SetValue(EmptyDataDesProperty, value);
        }



        #region 必填选项设置

        public static readonly DependencyProperty IsRequiredProperty =
  DependencyProperty.RegisterAttached("IsRequired", typeof(bool), typeof(ControlsHelper), new PropertyMetadata(false));

        public static bool GetIsRequired(UIElement element)
        {
            return (bool)element.GetValue(IsRequiredProperty);
        }
        public static void SetIsRequired(UIElement element, bool value)
        {
            element.SetValue(IsRequiredProperty, value);
        }
        #endregion


        public static readonly DependencyProperty IsShowValueProperty = DependencyProperty.RegisterAttached(
       "IsShowValue",
       typeof(bool),
       typeof(ControlsHelper),
       new FrameworkPropertyMetadata(false));

        [AttachedPropertyBrowsableForType(typeof(Button))]
        public static bool GetIsShowValue(UIElement element)
        {
            return (bool)element.GetValue(IsShowValueProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(Button))]
        public static void SetIsShowValue(UIElement element, bool value)
        {
            element.SetValue(IsShowValueProperty, value);
        }



        #region 选中颜色

        public static readonly DependencyProperty SelectedBackgroundProperty = DependencyProperty.RegisterAttached(
   "SelectedBackground",
   typeof(Brush),
   typeof(ControlsHelper),
   new FrameworkPropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEE"))));

        public static Brush GetSelectedBackground(UIElement element)
        {
            return (Brush)element.GetValue(SelectedBackgroundProperty);
        }

        public static void SetSelectedBackground(UIElement element, Brush value)
        {
            element.SetValue(SelectedBackgroundProperty, value);
        }
        #endregion




        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.RegisterAttached(
       "Description",
       typeof(string),
       typeof(ControlsHelper),
       new FrameworkPropertyMetadata(null));

        public static string GetDescription(UIElement element)
        {
            return (string)element.GetValue(DescriptionProperty);
        }

        public static void SetDescription(UIElement element, string value)
        {
            element.SetValue(DescriptionProperty, value);
        }


        public static readonly DependencyProperty DesForegroundProperty = DependencyProperty.RegisterAttached(
       "DesForeground",
       typeof(Brush),
       typeof(ControlsHelper),
       new FrameworkPropertyMetadata(null));

        public static Brush GetDesForeground(UIElement element)
        {
            return (Brush)element.GetValue(DesForegroundProperty);
        }

        public static void SetDesForeground(UIElement element, Brush value)
        {
            element.SetValue(DesForegroundProperty, value);
        }


      
        public static readonly DependencyProperty BlendRatioProperty =
            DependencyProperty.RegisterAttached(
                "BlendRatio",
                typeof(double),
                typeof(ControlsHelper),
                new PropertyMetadata(0D));

        public static void SetBlendRatio(UIElement element, double value)
        {
            element.SetValue(BlendRatioProperty, value);
        }

        public static double GetBlendRatio(UIElement element)
        {
            return (double)element.GetValue(BlendRatioProperty);
        }
    }
}
