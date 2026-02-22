using RS.Widgets.Enums;
using System;
using System.Windows;
using System.Windows.Media;


namespace RS.Widgets.Visuals
{
    /// <summary>
    /// 一个可复用的 DrawingVisual，用于渲染 RSTransformRig 的视觉样式：
    /// 边框、缩放/旋转的命中测试把手、方向箭头以及方向按钮。
    /// 可以将此 Visual 托管在 Adorner 或任何管理 VisualCollection 的容器中。
    /// </summary>
    public class RSTransformVisual : DrawingVisual
    {
        // ── 尺寸常量 (与 XAML Margin/Width/Height 匹配) ──

        private const double ArrowWidth = 20.0;         // 方向箭头的宽度
        private const double ArrowHeight = 32.0;        // 方向箭头的最小高度
        private const double ArrowStemWidth = 2.0;      // 箭头柄的宽度
        private const double ArrowMarginOffset = 20.0;  // 箭头向外延伸的距离
        private const double DirectionHostSize = 45.0;  // 方向按钮容器的宽度和高度
        private const double DirectionTriSize = 12.0;   // 方向三角按钮的尺寸
        private const double DirectionHostOffset = 10.0;// 方向按钮容器的边距偏移




        // 缓存的渲染资源
        private Pen FramePen;
        private Pen FramePenSelected;
        private Pen StemPen;
        private Pen HoverStrokePen;
        private Brush BorderBrush;

        // 缓存的几何图形
        private static readonly StreamGeometry TriangleTop;
        private static readonly StreamGeometry TriangleBottom;
        private static readonly StreamGeometry TriangleLeft;
        private static readonly StreamGeometry TriangleRight;

        static RSTransformVisual()
        {
            TriangleTop = CreateTriangleGeometry(0);
            TriangleBottom = CreateTriangleGeometry(180);
            TriangleLeft = CreateTriangleGeometry(-90);
            TriangleRight = CreateTriangleGeometry(90);
        }

        private static StreamGeometry CreateTriangleGeometry(double angleDeg)
        {
            double halfBase = DirectionTriSize / 2.0;
            double height = DirectionTriSize * 0.866;

            // 基础三角形向上 (中心点在原点)
            Point tip = new Point(0, -height / 2);
            Point bl = new Point(-halfBase, height / 2);
            Point br = new Point(halfBase, height / 2);

            // 旋转逻辑
            double rad = angleDeg * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);

            Point RotatePoint(Point p)
            {
                return new Point(
                    p.X * cos - p.Y * sin,
                    p.Y * cos + p.X * sin);
            }

            Point r0 = RotatePoint(tip);
            Point r1 = RotatePoint(bl);
            Point r2 = RotatePoint(br);

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(r0, true, true);
                ctx.LineTo(r1, true, false);
                ctx.LineTo(r2, true, false);
            }
            geometry.Freeze();
            return geometry;
        }


        /// <summary>
        /// 根据当前状态重绘所有视觉元素。
        /// </summary>
        /// <param name="size">被装饰元素的像素尺寸。</param>
        /// <param name="borderBrush">用于边框和方向指示器的画刷。</param>
        /// <param name="isSelect">元素是否被选中（边框加粗）。</param>
        /// <param name="isSingleSelect">是否显示方向按钮。</param>
        /// <param name="rectDirection">方向箭头指向的边。</param>
        /// <param name="isDirectionEnabled">是否启用了方向功能。</param>
        /// <param name="hoveredDirection">悬停的方向（可选）。</param>
        public void Render(Size size, Brush borderBrush, bool isSelect, bool isSingleSelect, RectDirection rectDirection, bool isDirectionEnabled, VisualOperation hoveredDirection = VisualOperation.None, bool showRotationCenter = false)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                // 清除之前的绘制
                using (var dc = RenderOpen())
                {
                }
                return;
            }

            // 如果画刷改变，则重建画笔
            if (BorderBrush != borderBrush)
            {
                BorderBrush = borderBrush ?? Brushes.DodgerBlue;
                RebuildPens();
            }

            double w = size.Width;
            double h = size.Height;

            using (var dc = RenderOpen())
            {
                // 1. 带有透明填充的矩形边框（提供移动命中测试区域和边框）
                Pen pen;
                if (isSelect)
                {
                    pen = FramePenSelected;
                }
                else
                {
                    pen = FramePen;
                }
                dc.DrawRectangle(Brushes.Transparent, pen, new Rect(0, 0, w, h));

                if (isDirectionEnabled)
                {
                    // 2. 绘制方向箭头（带有柄的箭头），柄的粗细与边框一致
                    DrawDirectionArrow(dc, w, h, rectDirection, isSelect);

                    // 3. 绘制方向按钮（仅在单选时显示）
                    if (isSingleSelect)
                    {
                        DrawDirectionButtons(dc, w, h, rectDirection, hoveredDirection);

                        // 4. 在悬停的方向绘制预览箭头（使用对比色）
                        if (hoveredDirection != VisualOperation.None)
                        {
                            RectDirection preDir = rectDirection;
                            switch (hoveredDirection)
                            {
                                case VisualOperation.ChangeDirectionTop:
                                    preDir = RectDirection.Top;
                                    break;
                                case VisualOperation.ChangeDirectionBottom:
                                    preDir = RectDirection.Bottom;
                                    break;
                                case VisualOperation.ChangeDirectionLeft:
                                    preDir = RectDirection.Left;
                                    break;
                                case VisualOperation.ChangeDirectionRight:
                                    preDir = RectDirection.Right;
                                    break;
                            }

                            if (preDir != rectDirection)
                            {
                                var contrastBrush = GetContrastBrush(BorderBrush);
                                var contrastStemPen = new Pen(contrastBrush, isSelect ? 2.0 : 1.0);
                                contrastStemPen.Freeze();
                                DrawDirectionArrow(dc, w, h, preDir, isSelect, contrastBrush, contrastStemPen);
                            }
                        }
                    }
                }

                if (showRotationCenter)
                {
                    DrawRotationCenter(dc, w, h);
                }
            }
        }


        private void RebuildPens()
        {
            var brush = BorderBrush ?? Brushes.DodgerBlue;

            FramePen = new Pen(brush, 1.0);
            FramePen.Freeze();

            FramePenSelected = new Pen(brush, 2.0);
            FramePenSelected.Freeze();

            StemPen = new Pen(brush, ArrowStemWidth);
            StemPen.Freeze();

            // 悬停边框画笔：使用对比色（更深或更浅）
            var contrastBrush = GetContrastBrush(brush);
            HoverStrokePen = new Pen(contrastBrush, 1.5);
            HoverStrokePen.Freeze();
        }

        /// <summary>
        /// 获取用于悬停边框的对比色画刷。
        /// 如果填充色较浅，则返回较深的颜色；如果较深，则返回较浅的颜色。
        /// </summary>
        private static Brush GetContrastBrush(Brush brush)
        {
            if (brush is SolidColorBrush scb)
            {
                var c = scb.Color;
                double luminance = 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
                double factor = luminance > 128 ? 0.5 : 1.8;
                byte r = (byte)Math.Min(255, c.R * factor);
                byte g = (byte)Math.Min(255, c.G * factor);
                byte b = (byte)Math.Min(255, c.B * factor);
                var contrast = new SolidColorBrush(Color.FromArgb(c.A, r, g, b));
                contrast.Freeze();
                return contrast;
            }
            return Brushes.White;
        }





        #region 方向箭头

        /// <summary>
        /// 绘制方向箭头：向上箭头图标 + 2像素的柄。
        /// 匹配 PART_RectDirectionArrow (20×32，位于边框外部)。
        /// </summary>
        private void DrawDirectionArrow(DrawingContext dc, double w, double h, RectDirection direction, bool isSelect, Brush brushOverride = null, Pen stemPenOverride = null)
        {
            var brush = brushOverride ?? BorderBrush ?? Brushes.DodgerBlue;
            Pen stemPen;
            if (stemPenOverride != null)
            {
                stemPen = stemPenOverride;
            }
            else
            {
                if (isSelect)
                {
                    stemPen = FramePenSelected;
                }
                else
                {
                    stemPen = FramePen;
                }
            }

            // 箭头局部区域：20宽 × 32高，在局部空间中箭头指向“上方”。
            // 对于每个方向，我们进行平移和旋转以正确定位。
            double cx = 0;
            double cy = 0;
            double rotation = 0;

            switch (direction)
            {
                case RectDirection.Top:
                    cx = w / 2;
                    cy = -ArrowMarginOffset + ArrowHeight / 2;
                    rotation = 0;
                    break;
                case RectDirection.Bottom:
                    cx = w / 2;
                    cy = h + ArrowMarginOffset - ArrowHeight / 2;
                    rotation = 180;
                    break;
                case RectDirection.Left:
                    cx = -ArrowMarginOffset + ArrowHeight / 2;
                    rotation = -90;
                    cy = h / 2;
                    break;
                case RectDirection.Right:
                    cx = w + ArrowMarginOffset - ArrowHeight / 2;
                    rotation = 90;
                    cy = h / 2;
                    break;
                default:
                    return;
            }

            // 变换：合并为一个 MatrixTransform 减少 Push 调用
            Matrix mat = Matrix.Identity;
            if (rotation != 0)
            {
                mat.Rotate(rotation);
            }
            mat.Translate(cx, cy);
            dc.PushTransform(new MatrixTransform(mat));

            // ── 在局部空间进行绘制 (指向上方，以原点为中心) ──
            double halfW = ArrowWidth / 2;
            double halfH = ArrowHeight / 2;

            // 1. 箭头柄：占据整个高度，水平居中
            dc.DrawLine(stemPen, new Point(0, -halfH), new Point(0, halfH));

            // 2. 箭头顶部倒 V 型
            double chevronH = ArrowWidth * 0.4;
            Point chevronTip = new Point(0, -halfH);
            Point chevronLeft = new Point(-halfW, -halfH + chevronH);
            Point chevronRight = new Point(halfW, -halfH + chevronH);
            dc.DrawLine(stemPen, chevronLeft, chevronTip);
            dc.DrawLine(stemPen, chevronTip, chevronRight);

            dc.Pop();
        }

        #endregion

        #region 方向按钮

        /// <summary>
        /// 绘制 4 个方向的三角按钮。
        /// 与当前方向匹配的按钮将被隐藏。
        /// </summary>
        private void DrawDirectionButtons(DrawingContext dc, double w, double h, RectDirection direction, VisualOperation hoveredDirection)
        {
            var brush = BorderBrush ?? Brushes.DodgerBlue;
            double cellSize = DirectionHostSize / 3.0;

            Point hostCenter = GetDirectionHostCenter(w, h, direction);

            double left = hostCenter.X - DirectionHostSize / 2;
            double top = hostCenter.Y - DirectionHostSize / 2;

            if (direction != RectDirection.Top)
            {
                bool isHovered = hoveredDirection == VisualOperation.ChangeDirectionTop;
                Point center = new Point(left + cellSize * 1.5, top + cellSize * 0.5);
                DrawTriangle(dc, TriangleTop, brush, center, isHovered);
            }

            if (direction != RectDirection.Left)
            {
                bool isHovered = hoveredDirection == VisualOperation.ChangeDirectionLeft;
                Point center = new Point(left + cellSize * 0.5, top + cellSize * 1.5);
                DrawTriangle(dc, TriangleLeft, brush, center, isHovered);
            }

            if (direction != RectDirection.Right)
            {
                bool isHovered = hoveredDirection == VisualOperation.ChangeDirectionRight;
                Point center = new Point(left + cellSize * 2.5, top + cellSize * 1.5);
                DrawTriangle(dc, TriangleRight, brush, center, isHovered);
            }

            if (direction != RectDirection.Bottom)
            {
                bool isHovered = hoveredDirection == VisualOperation.ChangeDirectionBottom;
                Point center = new Point(left + cellSize * 1.5, top + cellSize * 2.5);
                DrawTriangle(dc, TriangleBottom, brush, center, isHovered);
            }
        }

        private void DrawTriangle(DrawingContext dc, StreamGeometry geometry, Brush brush, Point center, bool isHovered)
        {
            dc.PushTransform(new TranslateTransform(center.X, center.Y));
            Pen strokePen = null;
            if (isHovered)
            {
                strokePen = HoverStrokePen;
            }
            dc.DrawGeometry(brush, strokePen, geometry);
            dc.Pop();
        }


        #endregion

        #region 命中测试

        // 命中测试的尺寸常量
        private const double ResizeHitSize = 10.0;
        private const double RotateHitSize = 20.0;

        /// <summary>
        /// 针对给定的点进行四周边框或角落的命中测试。
        /// 内部角点表示缩放，外部角点表示旋转，边缘表示缩放，主体内部表示移动。
        /// </summary>
        /// <param name="point">局部坐标系内的点。</param>
        /// <param name="size">被装饰元素的视觉尺寸。</param>
        /// <param name="currentDirection">当前的方向。</param>
        /// <param name="isDirectionEnabled">是否启用了方向功能。</param>
        /// <param name="isRotationEnabled">是否启用了旋转功能。</param>
        public VisualOperation GetVisualOperation(Point point, Size size, RectDirection currentDirection, bool isDirectionEnabled, bool isRotationEnabled = true)
        {
            double w = size.Width;
            double h = size.Height;

            // 角落中心点
            Point tl = new Point(0, 0);
            Point tr = new Point(w, 0);
            Point br = new Point(w, h);
            Point bl = new Point(0, h);

            // 1. 检查缩放角点（高优先级）
            if (IsPointInRect(point, tl, ResizeHitSize))
            {
                return VisualOperation.ResizeTopLeft;
            }
            if (IsPointInRect(point, tr, ResizeHitSize))
            {
                return VisualOperation.ResizeTopRight;
            }
            if (IsPointInRect(point, br, ResizeHitSize))
            {
                return VisualOperation.ResizeBottomRight;
            }
            if (IsPointInRect(point, bl, ResizeHitSize))
            {
                return VisualOperation.ResizeBottomLeft;
            }

            // 2. 检查旋转角点（次要优先级）
            if (isRotationEnabled)
            {
                if (IsPointInRect(point, tl, RotateHitSize))
                {
                    return VisualOperation.RotateTopLeft;
                }
                if (IsPointInRect(point, tr, RotateHitSize))
                {
                    return VisualOperation.RotateTopRight;
                }
                if (IsPointInRect(point, br, RotateHitSize))
                {
                    return VisualOperation.RotateBottomRight;
                }
                if (IsPointInRect(point, bl, RotateHitSize))
                {
                    return VisualOperation.RotateBottomLeft;
                }
            }

            // 3. 检查缩放边缘
            if (Math.Abs(point.Y) <= ResizeHitSize / 2 && point.X > 0 && point.X < w)
            {
                return VisualOperation.ResizeTop;
            }
            if (Math.Abs(point.Y - h) <= ResizeHitSize / 2 && point.X > 0 && point.X < w)
            {
                return VisualOperation.ResizeBottom;
            }
            if (Math.Abs(point.X) <= ResizeHitSize / 2 && point.Y > 0 && point.Y < h)
            {
                return VisualOperation.ResizeLeft;
            }
            if (Math.Abs(point.X - w) <= ResizeHitSize / 2 && point.Y > 0 && point.Y < h)
            {
                return VisualOperation.ResizeRight;
            }

            // 4. 检查主方向箭头（用于旋转）
            if (isDirectionEnabled && isRotationEnabled)
            {
                Rect? arrowRect = null;
                double aw = ArrowWidth;
                double ah = ArrowHeight;
                switch (currentDirection)
                {
                    case RectDirection.Top:
                        arrowRect = new Rect((w - aw) / 2, -ArrowMarginOffset, aw, ah);
                        break;

                    case RectDirection.Bottom:
                        arrowRect = new Rect((w - aw) / 2, h + ArrowMarginOffset - ah, aw, ah);
                        break;

                    case RectDirection.Left:
                        arrowRect = new Rect(-ArrowMarginOffset, (h - aw) / 2, ah, aw);
                        break;

                    case RectDirection.Right:
                        arrowRect = new Rect(w + ArrowMarginOffset - ah, (h - aw) / 2, ah, aw);
                        break;
                }
                if (arrowRect.HasValue && arrowRect.Value.Contains(point))
                {
                    return VisualOperation.Rotate;
                }
            }

            // 5. 检查主体内部（用于移动）
            if (point.X >= 0 && point.X <= w && point.Y >= 0 && point.Y <= h)
            {
                return VisualOperation.Move;
            }

            return VisualOperation.None;
        }

        private static bool IsPointInRect(Point p, Point center, double size)
        {
            return p.X >= center.X - size / 2 && p.X <= center.X + size / 2 &&
                   p.Y >= center.Y - size / 2 && p.Y <= center.Y + size / 2;
        }


        /// <summary>
        /// 针对 4 个方向的按钮进行命中测试。
        /// 返回命中的 VisualOperation，如果未命中则返回 VisualOperation.None。
        /// </summary>
        public VisualOperation GetDirectionButtonOperation(Point point, Size size, RectDirection currentDirection)
        {
            double w = size.Width;
            double h = size.Height;
            double cellSize = DirectionHostSize / 3.0;
            double hitRadius = cellSize;

            // 计算容器中心点
            Point hostCenter = GetDirectionHostCenter(w, h, currentDirection);

            double left = hostCenter.X - DirectionHostSize / 2;
            double top = hostCenter.Y - DirectionHostSize / 2;

            // 测试上方按钮
            if (currentDirection != RectDirection.Top)
            {
                Point center = new Point(left + cellSize * 1.5, top + cellSize * 0.5);
                if (IsPointInRect(point, center, hitRadius))
                {
                    return VisualOperation.ChangeDirectionTop;
                }
            }

            // 测试左侧按钮
            if (currentDirection != RectDirection.Left)
            {
                Point center = new Point(left + cellSize * 0.5, top + cellSize * 1.5);
                if (IsPointInRect(point, center, hitRadius))
                {
                    return VisualOperation.ChangeDirectionLeft;
                }
            }

            // 测试右侧按钮
            if (currentDirection != RectDirection.Right)
            {
                Point center = new Point(left + cellSize * 2.5, top + cellSize * 1.5);
                if (IsPointInRect(point, center, hitRadius))
                {
                    return VisualOperation.ChangeDirectionRight;
                }
            }

            // 测试下方按钮
            if (currentDirection != RectDirection.Bottom)
            {
                Point center = new Point(left + cellSize * 1.5, top + cellSize * 2.5);
                if (IsPointInRect(point, center, hitRadius))
                {
                    return VisualOperation.ChangeDirectionBottom;
                }
            }

            return VisualOperation.None;
        }

        #endregion

        #region Helpers

        private static Point GetDirectionHostCenter(double w, double h, RectDirection direction)
        {
            switch (direction)
            {
                case RectDirection.Top:
                    return new Point(w / 2, -DirectionHostOffset + DirectionHostSize / 2);
                case RectDirection.Bottom:
                    return new Point(w / 2, h + DirectionHostOffset - DirectionHostSize / 2);
                case RectDirection.Left:
                    return new Point(-DirectionHostOffset + DirectionHostSize / 2, h / 2);
                case RectDirection.Right:
                    return new Point(w + DirectionHostOffset - DirectionHostSize / 2, h / 2);
                default:
                    return new Point(w / 2, -DirectionHostOffset + DirectionHostSize / 2);
            }
        }

        private void DrawRotationCenter(DrawingContext dc, double w, double h)
        {
            double cx = w / 2;
            double cy = h / 2;
            double radius = 5.0;
            double crossSize = 8.0;

            // 绘制一个带有十字线的小圆圈来表示中心点
            Brush brush = BorderBrush ?? Brushes.DodgerBlue;
            Pen pen = new Pen(brush, 1.0);
            pen.Freeze();

            // 背景光环 (半透明，提高可见度)
            Brush shadowBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            shadowBrush.Freeze();
            dc.DrawEllipse(shadowBrush, null, new Point(cx, cy), radius + 2, radius + 2);

            // 中心圆点
            dc.DrawEllipse(brush, null, new Point(cx, cy), 2, 2);

            // 十字架线
            dc.DrawLine(pen, new Point(cx - crossSize / 2, cy), new Point(cx + crossSize / 2, cy));
            dc.DrawLine(pen, new Point(cx, cy - crossSize / 2), new Point(cx, cy + crossSize / 2));

            // 外圈
            dc.DrawEllipse(null, pen, new Point(cx, cy), radius, radius);
        }
        #endregion
    }
}
