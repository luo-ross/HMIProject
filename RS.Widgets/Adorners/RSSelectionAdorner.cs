using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace RS.Widgets.Adorners
{
    public class RSSelectionAdorner : Adorner
    {
        private RSSelectableTextBlock Control;

        public RSSelectionAdorner(RSSelectableTextBlock adornedElement) : base(adornedElement)
        {
            Control = adornedElement;
            this.IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Control.StartPosition == null || Control.EndPosition == null) return;
            var start = Control.StartPosition;
            var end = Control.EndPosition;
            if (start.CompareTo(end) > 0) { var t = start; start = end; end = t; }
            if (start.CompareTo(end) == 0) return;

            TextPointer p = start;
            while (p != null && p.CompareTo(end) < 0)
            {
                TextPointer lineEnd = p.GetLineStartPosition(1);
                if (lineEnd != null)
                {
                    lineEnd = lineEnd.GetNextInsertionPosition(LogicalDirection.Backward);
                }

                TextPointer chunkEnd = (lineEnd != null && lineEnd.CompareTo(end) < 0) ? lineEnd : end;

                if (chunkEnd.CompareTo(p) <= 0)
                {
                    p = p.GetLineStartPosition(1);
                    continue;
                }

                Rect rStart = p.GetCharacterRect(LogicalDirection.Forward);
                Rect rEnd = chunkEnd.GetCharacterRect(LogicalDirection.Backward);

                // 绘制此块的矩形
                Rect chunkRect = new Rect(rStart.TopLeft, rEnd.BottomRight);
                // 确保高度合理（取开始/结束的最大值）
                chunkRect.Height = Math.Max(rStart.Height, rEnd.Height);
                // 确保宽度为正（如果 RTL，rEnd 可能在左侧，但假设 LTR）
                if (chunkRect.Width < 0) chunkRect.Width = 0;

                drawingContext.DrawRectangle(Control.SelectionBrush, null, chunkRect);

                // 移动到下一行
                p = p.GetLineStartPosition(1);
            }
        }
    }
}
