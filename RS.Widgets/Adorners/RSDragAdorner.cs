using RS.Widgets.Adorners;
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
    public class RSDragAdorner : RSAdorner
    {

        public RSDragAdorner(FrameworkElement adornedElement) : base(adornedElement)
        {

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var adornedElement = this.AdornedElement as FrameworkElement;
            var actualWidth = adornedElement.ActualWidth;
            var actualHeight = adornedElement.ActualHeight;
            VisualBrush visualBrush = new VisualBrush(adornedElement);

            var mousePosition = Mouse.GetPosition(this);
            var x = mousePosition.X - actualWidth / 2;
            var y = mousePosition.Y - actualHeight / 2;
            Rect rect = new Rect(x, y, actualWidth, actualHeight);

            drawingContext.DrawRectangle(visualBrush, new Pen(), rect);
        }
    }
}
