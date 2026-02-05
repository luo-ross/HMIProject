using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
   
    public class AcrylicHelper
    {

        public static readonly DependencyProperty AmountProperty =
            DependencyProperty.RegisterAttached(
                "Amount",
                typeof(double),
                typeof(AcrylicHelper),
                new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.Inherits));



        public static double GetAmount(DependencyObject obj)
        {
            return (double)obj.GetValue(AmountProperty);
        }


        public static void SetAmount(DependencyObject obj, Color value)
        {
            obj.SetValue(AmountProperty, value);
        }




        public static readonly DependencyProperty TintColorProperty =
            DependencyProperty.RegisterAttached(
                "TintColor",
                typeof(Brush),
                typeof(AcrylicHelper),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), FrameworkPropertyMetadataOptions.Inherits));

        public static Brush GetTintColor(DependencyObject obj)
        {
            return (Brush)obj.GetValue(TintColorProperty);
        }


        public static void SetTintColor(DependencyObject obj, Brush value)
        {
            obj.SetValue(TintColorProperty, value);
        }




        public static readonly DependencyProperty TintOpacityProperty =
            DependencyProperty.RegisterAttached(
                "TintOpacity",
                typeof(double),
                typeof(AcrylicHelper),
                new FrameworkPropertyMetadata(0.8, FrameworkPropertyMetadataOptions.Inherits));

        public static double GetTintOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(TintOpacityProperty);
        }


        public static void SetTintOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(TintOpacityProperty, value);
        }





        public static readonly DependencyProperty NoiseOpacityProperty =
            DependencyProperty.RegisterAttached(
                "NoiseOpacity",
                typeof(double),
                typeof(AcrylicHelper),
                new FrameworkPropertyMetadata(0.03, FrameworkPropertyMetadataOptions.Inherits));
        public static double GetNoiseOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(NoiseOpacityProperty);
        }

        public static void SetNoiseOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(NoiseOpacityProperty, value);
        }




        public static readonly DependencyProperty FallbackColorProperty =
            DependencyProperty.RegisterAttached(
                "FallbackColor",
                typeof(Color),
                typeof(AcrylicHelper),
                new FrameworkPropertyMetadata(Colors.LightGray, FrameworkPropertyMetadataOptions.Inherits));
        public static Color GetFallbackColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(FallbackColorProperty);
        }

        public static void SetFallbackColor(DependencyObject obj, Color value)
        {
            obj.SetValue(FallbackColorProperty, value);
        }



    }
}
