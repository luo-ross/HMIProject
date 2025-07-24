using RS.Widgets.Commons;
using RS.Widgets.Structs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    [TemplatePart(Name = nameof(PART_ColorSelectThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(PART_HouColorThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(PART_ColorSelectCanvas), Type = typeof(Canvas))]
    [TemplatePart(Name = nameof(PART_HouColorCanvas), Type = typeof(Canvas))]
    public class RSColorPicker : ContentControl
    {
        private Thumb PART_ColorSelectThumb;
        private Thumb PART_HouColorThumb;
        private Canvas PART_ColorSelectCanvas;
        private Canvas PART_HouColorCanvas;
        static RSColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSColorPicker), new FrameworkPropertyMetadata(typeof(RSColorPicker)));
        }


        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.SetDefaultValues();
            this.UpdateHouColor();
        }

        private void SetDefaultValues()
        {
            Canvas.SetLeft(this.PART_ColorSelectThumb,this.PART_ColorSelectCanvas.ActualWidth- this.PART_ColorSelectThumb.ActualWidth/2);
            Canvas.SetTop(this.PART_ColorSelectThumb, 0);
        }


        public event Action<Color> OnColorSelect;

        public bool IsSelftUpdateColorSelect { get; set; }
        public HSB HSB
        {
            get { return (HSB)GetValue(HSBProperty); }
            set { SetValue(HSBProperty, value); }
        }

        public static readonly DependencyProperty HSBProperty =
            DependencyProperty.Register("HSB", typeof(HSB), typeof(RSColorPicker), new PropertyMetadata(default));





        public Point HouColorMouseMovePosition
        {
            get { return (Point)GetValue(HouColorMouseMovePositionProperty); }
            set { SetValue(HouColorMouseMovePositionProperty, value); }
        }

        public static readonly DependencyProperty HouColorMouseMovePositionProperty =
            DependencyProperty.Register("HouColorMouseMovePosition", typeof(Point), typeof(RSColorPicker), new PropertyMetadata(default));




        public Point ColorSelectMouseMovePosition
        {
            get { return (Point)GetValue(ColorSelectMouseMovePositionProperty); }
            set { SetValue(ColorSelectMouseMovePositionProperty, value); }
        }

        public static readonly DependencyProperty ColorSelectMouseMovePositionProperty =
            DependencyProperty.Register("ColorSelectMouseMovePosition", typeof(Point), typeof(RSColorPicker), new PropertyMetadata(default));




        public Color HouColor
        {
            get { return (Color)GetValue(HouColorProperty); }
            set { SetValue(HouColorProperty, value); }
        }

        public static readonly DependencyProperty HouColorProperty =
            DependencyProperty.Register("HouColor", typeof(Color), typeof(RSColorPicker), new PropertyMetadata(default));



        public Color ColorSelect
        {
            get { return (Color)GetValue(ColorSelectProperty); }
            set { SetValue(ColorSelectProperty, value); }
        }

        public static readonly DependencyProperty ColorSelectProperty =
            DependencyProperty.Register("ColorSelect", typeof(Color), typeof(RSColorPicker), new PropertyMetadata(default(Color), OnColorSelectPropertyChanged));

        private static void OnColorSelectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsColorPicker = d as RSColorPicker;
            var color = (Color)e.NewValue;
            rsColorPicker.HandleColorSelectChangedEvent(color);
        }

        /// <summary>
        /// 处理颜色改变事件
        /// </summary>
        private void HandleColorSelectChangedEvent(Color color)
        {
            if (this.IsSelftUpdateColorSelect)
            {
                this.IsSelftUpdateColorSelect = false;
                return;
            }

            if (this.PART_ColorSelectCanvas == null)
            {
                return;
            }

            double h = 0, s = 0, b = 0;
            ColorHelper.HsbFromColor(color, ref h, ref s, ref b);
            this.HSB = new HSB()
            {
                H = h,
                S = s,
                B = b
            };
            this.HouColor = ColorHelper.ColorFromHsb(this.HSB.H, 1, 1);
            Canvas.SetLeft(this.PART_ColorSelectThumb, s * this.PART_ColorSelectCanvas.ActualWidth - this.PART_ColorSelectThumb.ActualWidth / 2);
            Canvas.SetTop(this.PART_ColorSelectThumb, (1 - b) * this.PART_ColorSelectCanvas.ActualHeight - this.PART_ColorSelectThumb.ActualHeight / 2);
            Canvas.SetTop(this.PART_HouColorThumb, h * this.PART_HouColorCanvas.ActualHeight - this.PART_HouColorThumb.ActualHeight / 2);

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_ColorSelectCanvas = GetTemplateChild(nameof(this.PART_ColorSelectCanvas)) as Canvas;
            this.PART_HouColorCanvas = GetTemplateChild(nameof(this.PART_HouColorCanvas)) as Canvas;
            this.PART_ColorSelectThumb = GetTemplateChild(nameof(this.PART_ColorSelectThumb)) as Thumb;
            this.PART_HouColorThumb = GetTemplateChild(nameof(this.PART_HouColorThumb)) as Thumb;

            if (this.PART_ColorSelectCanvas != null)
            {
                this.PART_ColorSelectCanvas.MouseMove -= PART_ColorSelectCanvas_MouseMove;
                this.PART_ColorSelectCanvas.MouseMove += PART_ColorSelectCanvas_MouseMove;

                this.PART_ColorSelectCanvas.MouseLeftButtonUp -= PART_ColorSelectCanvas_MouseLeftButtonUp;
                this.PART_ColorSelectCanvas.MouseLeftButtonUp += PART_ColorSelectCanvas_MouseLeftButtonUp;
            }


            if (this.PART_HouColorCanvas != null)
            {
                this.PART_HouColorCanvas.MouseMove -= PART_HouColorCanvas_MouseMove;
                this.PART_HouColorCanvas.MouseMove += PART_HouColorCanvas_MouseMove;

                this.PART_HouColorCanvas.MouseLeftButtonUp -= PART_HouColorCanvas_MouseLeftButtonUp;
                this.PART_HouColorCanvas.MouseLeftButtonUp += PART_HouColorCanvas_MouseLeftButtonUp;
            }

            if (this.PART_ColorSelectThumb != null)
            {
                this.PART_ColorSelectThumb.DragDelta -= PART_ColorSelectThumb_DragDelta;
                this.PART_ColorSelectThumb.DragDelta += PART_ColorSelectThumb_DragDelta;
                this.PART_ColorSelectThumb.DragCompleted -= PART_ColorSelectThumb_DragCompleted;
                this.PART_ColorSelectThumb.DragCompleted += PART_ColorSelectThumb_DragCompleted;
            }

            if (this.PART_HouColorThumb != null)
            {
                this.PART_HouColorThumb.DragDelta -= PART_HouColorThumb_DragDelta;
                this.PART_HouColorThumb.DragDelta += PART_HouColorThumb_DragDelta;
                this.PART_HouColorThumb.DragCompleted -= PART_HouColorThumb_DragCompleted;
                this.PART_HouColorThumb.DragCompleted += PART_HouColorThumb_DragCompleted;
            }

           
        }

        private void PART_HouColorThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {

        }

        private void PART_HouColorThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var top = Canvas.GetTop(this.PART_HouColorThumb) + e.VerticalChange;
            this.UpdateHouColorThumbPosition(top);
            this.IsSelftUpdateColorSelect = true;
            this.OnColorSelect?.Invoke(this.ColorSelect);
        }

        private void UpdateHouColorThumbPosition(double top)
        {
            top = Math.Max(-this.PART_HouColorThumb.ActualHeight / 2D, top);
            top = Math.Min(this.PART_HouColorCanvas.ActualHeight - this.PART_HouColorThumb.ActualHeight / 2D, top);
            Canvas.SetTop(this.PART_HouColorThumb, top);
            this.UpdateHouColor();
        }

        private void UpdateHouColor()
        {
            var top = Canvas.GetTop(this.PART_HouColorThumb) + this.PART_HouColorThumb.ActualHeight / 2D;
            var offset = top / this.PART_HouColorCanvas.ActualHeight;
            offset = Math.Max(0, Math.Min(1, offset));
            this.HSB = new HSB()
            {
                H = offset,
                S = this.HSB.S,
                B = this.HSB.B
            };
            this.HouColor = ColorHelper.ColorFromHsb(this.HSB.H, 1, 1);
            this.UpdateColorSelect();
        }

        private void PART_ColorSelectThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
        }

        private void PART_ColorSelectThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var left = Canvas.GetLeft(this.PART_ColorSelectThumb) + e.HorizontalChange;
            var top = Canvas.GetTop(this.PART_ColorSelectThumb) + e.VerticalChange;
            this.UpdateColorSelectThumbPosition(left, top);
            this.IsSelftUpdateColorSelect = true;
            this.OnColorSelect?.Invoke(this.ColorSelect);
        }

        private void UpdateColorSelectThumbPosition(double left, double top)
        {
            left = Math.Max(-this.PART_ColorSelectThumb.ActualWidth / 2D, left);
            left = Math.Min(this.PART_ColorSelectCanvas.ActualWidth - this.PART_ColorSelectThumb.ActualWidth / 2D, left);
            top = Math.Max(-this.PART_ColorSelectThumb.ActualHeight / 2D, top);
            top = Math.Min(this.PART_ColorSelectCanvas.ActualHeight - this.PART_ColorSelectThumb.ActualHeight / 2D, top);
            Canvas.SetLeft(this.PART_ColorSelectThumb, left);
            Canvas.SetTop(this.PART_ColorSelectThumb, top);

            this.UpdateColorSelect();
        }

        private void UpdateColorSelect()
        {
            var left = Canvas.GetLeft(this.PART_ColorSelectThumb);
            left = left + this.PART_ColorSelectThumb.ActualWidth / 2D;
            var top = Canvas.GetTop(this.PART_ColorSelectThumb);
            top = top + this.PART_ColorSelectThumb.ActualHeight / 2D;

            this.HSB = new HSB()
            {
                H = this.HSB.H,
                S = left / this.PART_ColorSelectCanvas.ActualWidth,
                B = 1 - top / this.PART_ColorSelectCanvas.ActualHeight,
            };

            var currentColor = ColorHelper.ColorFromAhsb(1, this.HSB.H, this.HSB.S, this.HSB.B);
            if (currentColor != ColorSelect)
            {
                this.ColorSelect = currentColor;
            }
        }

        private void PART_HouColorCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.UpdateHouColorThumbPosition(this.HouColorCanvasMouseMovePosition.Y-this.PART_HouColorThumb.ActualHeight/2);
            this.OnColorSelect?.Invoke(this.ColorSelect);
        }


        public Point HouColorCanvasMouseMovePosition { get; set; }
        private void PART_HouColorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            this.HouColorCanvasMouseMovePosition = e.GetPosition(this.PART_HouColorCanvas);
        }

        private void PART_ColorSelectCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.UpdateColorSelectThumbPosition(this.ColorSelectCanvasMouseMovePosition.X-this.PART_ColorSelectThumb.ActualWidth/2, this.ColorSelectCanvasMouseMovePosition.Y - this.PART_ColorSelectThumb.ActualHeight / 2);
            this.OnColorSelect?.Invoke(this.ColorSelect);
        }


        public Point ColorSelectCanvasMouseMovePosition { get; set; }
        private void PART_ColorSelectCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.ColorSelectCanvasMouseMovePosition = e.GetPosition(this.PART_ColorSelectCanvas);
        }
    }
}
