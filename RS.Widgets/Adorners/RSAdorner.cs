using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.Widgets.Adorners
{
    public class RSAdorner : Adorner
    {
        public Window ParentWin;
        public Point CurrentMouseWinPoint;
        public RSAdorner(FrameworkElement adornedElement) : base(adornedElement)
        {
            Loaded += RSAdorner_Loaded;
            Unloaded += RSAdorner_Unloaded;
        }

        private void RSAdorner_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ParentWin != null)
            {
                ParentWin.PreviewMouseLeftButtonUp -= ParentWin_PreviewMouseLeftButtonUp;
                ParentWin.PreviewMouseMove -= ParentWin_PreviewMouseMove;
            }
        }

        private void RSAdorner_Loaded(object sender, RoutedEventArgs e)
        {
            this.ParentWin = Window.GetWindow(this);
            this.CurrentMouseWinPoint = Mouse.GetPosition(this.ParentWin);
            if (this.ParentWin != null)
            {
                this.ParentWin.PreviewMouseLeftButtonUp += ParentWin_PreviewMouseLeftButtonUp;
                this.ParentWin.PreviewMouseMove += ParentWin_PreviewMouseMove;
            }
        }
      

        private void ParentWin_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            this.CurrentMouseWinPoint = e.GetPosition(this.ParentWin);
            this.OnParentWin_PreviewMouseMove(sender, e);
            this.InvalidateVisual();
        }

        private void ParentWin_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.OnParentWin_PreviewMouseLeftButtonUp(sender, e);
            var adornerLayer = Parent as AdornerLayer;
            adornerLayer?.Remove(this);
        }

        public virtual void OnParentWin_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }
        public virtual void OnParentWin_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }


        public static T GetUIElementUnderMouse<T>(UIElement uIElement, Point mousePosition) where T : UIElement
        {
            var hitTestResult = VisualTreeHelper.HitTest(uIElement, mousePosition);
            var visualHit = hitTestResult?.VisualHit;

            while (visualHit != null)
            {
                var child = visualHit as T;
                if (child != null)
                {
                    return child;
                }
                visualHit = VisualTreeHelper.GetParent(visualHit);
            }
            return null;
        }


        

    }
}
