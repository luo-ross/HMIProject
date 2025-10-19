using RS.Win32API;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace RS.Widgets.Controls
{

    public class RSDragEffect : Window
    {
        static RSDragEffect()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDragEffect), new FrameworkPropertyMetadata(typeof(RSDragEffect)));
        }
        public RSDragEffect() 
        {

        }

        public DragDropEffects DragDropEffects
        {
            get { return (DragDropEffects)GetValue(DragDropEffectsProperty); }
            set { SetValue(DragDropEffectsProperty, value); }
        }

        public static readonly DependencyProperty DragDropEffectsProperty =
            DependencyProperty.Register("DragDropEffects", typeof(DragDropEffects), typeof(RSDragEffect), new PropertyMetadata(DragDropEffects.None));



        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(RSDragEffect), new PropertyMetadata(20D));


        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }

        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(RSDragEffect), new PropertyMetadata(20D));

        public void UpdateDragDropEffects(DragDropEffects dragDropEffects)
        {
            this.DragDropEffects = dragDropEffects;
        }


    }
}
