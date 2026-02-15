using RS.Widgets.Adorners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class TransformHelper
    {

        public static readonly DependencyProperty IsMonitorProperty =
            DependencyProperty.RegisterAttached(
                "IsMonitor",
                typeof(bool),
                typeof(TransformHelper),
                new FrameworkPropertyMetadata(false,  OnIsMonitorPropertyChanged));

        private static void OnIsMonitorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var targetElement = d as FrameworkElement;
            targetElement.Loaded += TargetElement_Loaded;
            targetElement.Unloaded += TargetElement_Unloaded;
        }

        private static void TargetElement_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private static void TargetElement_Loaded(object sender, RoutedEventArgs e)
        {
            var d = sender as UIElement;
            var window = Window.GetWindow(d);
            // 获取Adorner层
            var adornerLayer = AdornerLayer.GetAdornerLayer(d);
            // 添加新的Adorner
            var transformAdorner = new TransformAdorner(d);
            adornerLayer.Add(transformAdorner);
        }

        public static bool GetIsMonitor(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitorProperty);
        }

        public static void SetIsMonitor(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitorProperty, value);
        }





    }
}
