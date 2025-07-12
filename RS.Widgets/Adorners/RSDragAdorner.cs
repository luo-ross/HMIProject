using RS.Widgets.Adorners;
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
            Rect rect = new Rect(0, 0, actualWidth, actualHeight);
            if (this.CurrentMousePoint != default)
            {
                rect.X = this.CurrentMousePoint.X - actualWidth / 2;
                rect.Y = this.CurrentMousePoint.Y - actualHeight / 2;
            }
            drawingContext.DrawRectangle(visualBrush, new Pen(), rect);
        }
    }
}
