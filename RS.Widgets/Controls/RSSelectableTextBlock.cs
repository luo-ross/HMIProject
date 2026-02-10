using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using RS.Widgets.Adorners;

namespace RS.Widgets.Controls
{
    public class RSSelectableTextBlock : TextBlock
    {
        private TextPointer StartPositionField;
        private TextPointer EndPositionField;
        private SelectionAdorner Adorner;
        private bool IsSelecting;

        static RSSelectableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSSelectableTextBlock), new FrameworkPropertyMetadata(typeof(RSSelectableTextBlock)));
            CursorProperty.OverrideMetadata(typeof(RSSelectableTextBlock), new FrameworkPropertyMetadata(Cursors.IBeam));
            FocusableProperty.OverrideMetadata(typeof(RSSelectableTextBlock), new FrameworkPropertyMetadata(true));
        }

        public RSSelectableTextBlock()
        {
            this.Loaded += RSSelectableTextBlock_Loaded;
            this.Unloaded += RSSelectableTextBlock_Unloaded;

            // 添加复制命令绑定
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, ExecuteCopy, CanExecuteCopy));
        }

        private void RSSelectableTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(this);
            if (layer != null)
            {
                Adorner = new SelectionAdorner(this);
                layer.Add(Adorner);
            }
        }

        private void RSSelectableTextBlock_Unloaded(object sender, RoutedEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(this);
            if (layer != null && Adorner != null)
            {
                layer.Remove(Adorner);
                Adorner = null;
            }
        }

        #region Properties

        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(RSSelectableTextBlock),
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(100, 51, 153, 255)), FrameworkPropertyMetadataOptions.AffectsRender, OnSelectionBrushChanged));

        public Brush SelectionBrush
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }

        private static void OnSelectionBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RSSelectableTextBlock)d).Adorner?.InvalidateVisual();
        }

        public string SelectedText
        {
            get
            {
                if (StartPositionField == null || EndPositionField == null) return string.Empty;
                var range = new TextRange(StartPositionField, EndPositionField);
                return range.Text;
            }
        }

        #endregion

        #region Mouse Events

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left)
            {
                this.Focus();
                this.CaptureMouse();
                IsSelecting = true;
                Point point = e.GetPosition(this);
                try
                {
                    StartPositionField = this.GetPositionFromPoint(point, true);
                    EndPositionField = StartPositionField;
                    UpdateAdorner();
                }
                catch (ArgumentException)
                {
                }
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsSelecting)
            {
                Point point = e.GetPosition(this);
                try
                {
                    EndPositionField = this.GetPositionFromPoint(point, true);
                    UpdateAdorner();
                }
                catch (ArgumentException)
                {
                }
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (IsSelecting)
            {
                IsSelecting = false;
                this.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            StartPositionField = null;
            EndPositionField = null;
            IsSelecting = false;
            UpdateAdorner();
        }
        
        #endregion

        #region Selection Logic

        private void UpdateAdorner()
        {
            if (Adorner != null)
            {
                Adorner.InvalidateVisual();
            }
        }

        internal List<Geometry> GetSelectionGeometries()
        {
            var geometries = new List<Geometry>();
            if (StartPositionField == null || EndPositionField == null || StartPositionField.CompareTo(EndPositionField) == 0)
            {
                return geometries;
            }

            TextPointer start = StartPositionField;
            TextPointer end = EndPositionField;

            if (start.CompareTo(end) > 0)
            {
                var temp = start;
                start = end;
                end = temp;
            }
           

            TextPointer current = start;
            while (current != null && current.CompareTo(end) < 0)
            {
                 if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                 {
                    
                    TextPointer next = current.GetNextContextPosition(LogicalDirection.Forward);
                    if (next == null) break;
                    if (next.CompareTo(end) > 0) next = end;

                    var rect1 = current.GetCharacterRect(LogicalDirection.Forward);
                    var rect2 = next.GetCharacterRect(LogicalDirection.Backward);
                    
                    // 简单情况：同一行
                    if (Math.Abs(rect1.Top - rect2.Top) < 1) 
                    {
                        geometries.Add(new RectangleGeometry(new Rect(rect1.TopLeft, rect2.BottomRight)));
                    }
                    else
                    {
                      
                    }
                    
                    current = next;
                 }
                 else
                 {
                     current = current.GetNextContextPosition(LogicalDirection.Forward);
                 }
            }
            
            
            return CalculateGeometries(start, end);

        }

        private List<Geometry> CalculateGeometries(TextPointer start, TextPointer end)
        {
             List<Geometry> geometryList = new List<Geometry>();
            
             TextPointer p = start;
             
             while (p != null && p.CompareTo(end) < 0)
             {
                 if (p.IsAtInsertionPosition)
                 {
                    Rect r = p.GetCharacterRect(LogicalDirection.Forward);
                    
                    TextPointer nextLine = p.GetLineStartPosition(1);
                    TextPointer segmentEnd = (nextLine != null && nextLine.CompareTo(end) < 0) ? nextLine : end;
                    
                    Rect rEnd = segmentEnd.GetCharacterRect(LogicalDirection.Backward);
                    
                     Rect unionRect = Rect.Empty;
                     
                     TextPointer traveler = p;
                     while(traveler.CompareTo(segmentEnd) < 0)
                     {
                         Rect charRect = traveler.GetCharacterRect(LogicalDirection.Forward);
                         if(unionRect == Rect.Empty) unionRect = charRect;
                         else unionRect.Union(charRect);
                         
                         traveler = traveler.GetNextInsertionPosition(LogicalDirection.Forward);
                         if(traveler == null) break;
                     }
                     
                     if (unionRect != Rect.Empty)
                     {
                         geometryList.Add(new RectangleGeometry(unionRect));
                     }

                     p = segmentEnd;
                 }
                 else
                 {
                     p = p.GetNextInsertionPosition(LogicalDirection.Forward);
                 }
             }

             return geometryList;
        }

     
        #endregion

        #region Copy Command

        private void ExecuteCopy(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedText))
            {
                Clipboard.SetText(SelectedText);
            }
        }

        private void CanExecuteCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty(SelectedText);
        }

        #endregion

        public TextPointer StartPosition
        {
            get { return StartPositionField; }
            internal set { StartPositionField = value; }
        }

        public TextPointer EndPosition
        {
            get { return EndPositionField; }
            internal set { EndPositionField = value; }
        }
    }
}
