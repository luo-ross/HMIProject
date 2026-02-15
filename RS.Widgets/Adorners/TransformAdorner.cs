using RS.Widgets.Adorners;
using RS.Widgets.Controls;
using RS.Widgets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RS.Widgets.Adorners
{
    public class TransformAdorner : Adorner
    {

        private static TransformSelectService TransformSelectService;

        static TransformAdorner()
        {
           TransformSelectService = new TransformSelectService();
        }

        public TransformAdorner(UIElement adornedElement) : base(adornedElement)
        {
           
            // 启用鼠标捕获
            //IsHitTestVisible = true;
            //Cursor = Cursors.SizeAll;
            this.MouseLeftButtonDown += TransformAdorner_MouseLeftButtonDown;
            this.MouseMove += TransformAdorner_MouseMove;
            this.MouseLeftButtonUp += TransformAdorner_MouseLeftButtonUp;
        }

     

        public bool IsSelect
        {
            get { return (bool)GetValue(IsSelectProperty); }
            set { SetValue(IsSelectProperty, value); }
        }

        public static readonly DependencyProperty IsSelectProperty =
            DependencyProperty.Register(nameof(IsSelect), typeof(bool), typeof(TransformAdorner), new PropertyMetadata(false, OnIsSelectPropertyChanged));

        private static void OnIsSelectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transformAdorner = d as TransformAdorner;
            transformAdorner.InvalidateVisual();
        }

        private void TransformAdorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            TransformSelectService.SingleSelect(this);
            Console.WriteLine("TransformAdorner_MouseLeftButtonDown");
            this.InvalidateVisual();
        }

        private void TransformAdorner_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            Console.WriteLine("TransformAdorner_MouseLeftButtonUp");
        }

        private void TransformAdorner_MouseMove(object sender, MouseEventArgs e)
        {
            var inputElement = sender as FrameworkElement;
            if (inputElement == null)
            {
                return;
            }
            this.UpdateCursor(inputElement, e);
            Console.WriteLine($"TransformAdorner_MouseMove ");
        }

        private void UpdateCursor(FrameworkElement inputElement, MouseEventArgs e)
        {
            if (!this.IsSelect)
            {
                this.Cursor = Cursors.Arrow;
                return;
            }

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                return;
            }
          
            Console.WriteLine("UpdateCursor");

            var mouseMovePosition = e.GetPosition(inputElement);
            var h = inputElement.ActualHeight;
            var w = inputElement.ActualWidth;
            var x = mouseMovePosition.X;
            var y = mouseMovePosition.Y;

            int borderThickness = 3;

            if (x >= borderThickness && x < w - borderThickness && y >= 0 && y < borderThickness)
            {
                this.Cursor = Cursors.SizeNS;
            }
            else if (x >= borderThickness && x < w - borderThickness && y >= h - borderThickness && y <= h)
            {
                this.Cursor = Cursors.SizeNS;
            }
            else if (x >= 0 && x < borderThickness && y >= borderThickness && y < h - borderThickness)
            {
                this.Cursor = Cursors.SizeWE;
            }
            else if (x >= w - borderThickness && x <= w && y >= borderThickness && y < h - borderThickness)
            {
                this.Cursor = Cursors.SizeWE;
            }
            else if (x >= 0 && x < borderThickness && y >= 0 && y < borderThickness)
            {
                this.Cursor = Cursors.SizeNWSE;
            }
            else if (x >= 0 && x < borderThickness && y >= h - borderThickness && y <= h)
            {
                this.Cursor = Cursors.SizeNESW;
            }
            else if (x >= w - borderThickness && x <= w && y >= 0 && y < borderThickness)
            {
                this.Cursor = Cursors.SizeNESW;
            }
            else if (x >= w - borderThickness && x <= w && y >= h - borderThickness && y <= h)
            {
                this.Cursor = Cursors.SizeNWSE;
            }
            else if (x >= borderThickness && x < w - borderThickness && y >= borderThickness && y < h - borderThickness)
            {
                this.Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Rect renderRect = new Rect(AdornedElement.RenderSize);
            Pen? pen = null;
            if (this.IsSelect)
            {
                pen = new Pen(Brushes.Red, 1) { DashStyle = DashStyles.Dash };
            }
            drawingContext.DrawRectangle(Brushes.Transparent, pen, renderRect);


        }

       

    }
}
