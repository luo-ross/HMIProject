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


        // 静态图标几何图形 (来自 Controls.Icons.xaml)
        private static readonly Geometry UpArrowGeo;

        static RSTransformVisual()
        {
            // 向上的箭头几何图形
            UpArrowGeo = Geometry.Parse(
                "M506.123 361.692L131.357 736.459c-12.497 12.496-32.758 12.496-45.255 0" +
                "-12.497-12.497-12.497-32.759 0-45.255l403.05-403.051c12.498-12.497 32.759" +
                "-12.497 45.256 0l403.05 403.05c12.497 12.497 12.497 32.759 0 45.256-12.496" +
                " 12.496-32.758 12.496-45.254 0L517.437 361.692a8 8 0 0 0-11.314 0z");
            UpArrowGeo.Freeze();
        }


        // 缓存的渲染资源
        private Pen FramePen;
        private Pen FramePenSelected;
        private Pen StemPen;
        private Pen HoverStrokePen;
        private Brush BorderBrush;


        /// <summary>
        /// 根据当前状态重绘所有视觉元素。
        /// </summary>
        /// <param name="size">被装饰元素的像素尺寸。</param>
        /// <param name="borderBrush">用于边框和方向指示器的画刷。</param>
        /// <param name="isSelect">元素是否被选中（边框加粗）。</param>
        /// <param name="isSingleSelect">是否显示方向按钮。</param>
        /// <param name="rectDirection">方向箭头指向的边。</param>
        /// <param name="hoveredDirection">悬停的方向（可选）。</param>
        public void Render(Size size, Brush borderBrush, bool isSelect, bool isSingleSelect, RectDirection rectDirection, RectDirection? hoveredDirection = null)
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
                var pen = isSelect ? FramePenSelected : FramePen;
                dc.DrawRectangle(Brushes.Transparent, pen, new Rect(0, 0, w, h));

                // 2. 绘制方向箭头（带有柄的箭头），柄的粗细与边框一致
                DrawDirectionArrow(dc, w, h, rectDirection, isSelect);

                // 3. 绘制方向按钮（仅在单选时显示）
                if (isSingleSelect)
                {
                    DrawDirectionButtons(dc, w, h, rectDirection, hoveredDirection);

                    // 4. 在悬停的方向绘制预览箭头（使用对比色）
                    if (hoveredDirection.HasValue && hoveredDirection.Value != rectDirection)
                    {
                        var contrastBrush = GetContrastBrush(BorderBrush);
                        var contrastStemPen = new Pen(contrastBrush, isSelect ? 2.0 : 1.0);
                        DrawDirectionArrow(dc, w, h, hoveredDirection.Value, isSelect, contrastBrush, contrastStemPen);
                    }
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
            var stemPen = stemPenOverride ?? (isSelect ? FramePenSelected : FramePen);

            // 箭头局部区域：20宽 × 32高，在局部空间中箭头指向“上方”。
            // 对于每个方向，我们进行平移和旋转以正确定位。
            double cx;
            double cy;
            double rotation;

            switch (direction)
            {
                case RectDirection.Top:
                    // 居中靠上，箭头延伸至上边缘上方
                    cx = w / 2;
                    cy = -ArrowMarginOffset + ArrowHeight / 2;
                    rotation = 0;
                    break;

                case RectDirection.Bottom:
                    // 居中靠下，箭头延伸至下边缘下方
                    cx = w / 2;
                    cy = h + ArrowMarginOffset - ArrowHeight / 2;
                    rotation = 180;
                    break;

                case RectDirection.Left:
                    // 靠左居中，箭头延伸至左边缘左侧
                    cx = -ArrowMarginOffset + ArrowHeight / 2;
                    cy = h / 2;
                    rotation = -90;
                    break;

                case RectDirection.Right:
                    // 靠右居中，箭头延伸至右边缘右侧
                    cx = w + ArrowMarginOffset - ArrowHeight / 2;
                    cy = h / 2;
                    rotation = 90;
                    break;

                default:
                    return;
            }

            // 变换：平移至中心，然后旋转
            dc.PushTransform(new TranslateTransform(cx, cy));
            if (rotation != 0)
            {
                dc.PushTransform(new RotateTransform(rotation));
            }

            // ── 在局部空间进行绘制 (指向上方，以原点为中心) ──
            double halfW = ArrowWidth / 2;
            double halfH = ArrowHeight / 2;

            // 1. 箭头柄：2像素宽，占据整个高度，水平居中
            dc.DrawLine(stemPen, new Point(0, -halfH), new Point(0, halfH));

            // 2. 箭头顶部倒 V 型
            double chevronH = ArrowWidth * 0.4;
            Point chevronTip = new Point(0, -halfH);
            Point chevronLeft = new Point(-halfW, -halfH + chevronH);
            Point chevronRight = new Point(halfW, -halfH + chevronH);
            dc.DrawLine(stemPen, chevronLeft, chevronTip);
            dc.DrawLine(stemPen, chevronTip, chevronRight);

            // 恢复变换
            if (rotation != 0)
            {
                dc.Pop();
            }
            dc.Pop();
        }

        #endregion

        #region 方向按钮

        /// <summary>
        /// 绘制 4 个方向的三角按钮。
        /// 与当前方向匹配的按钮将被隐藏。
        /// </summary>
        private void DrawDirectionButtons(DrawingContext dc, double w, double h, RectDirection direction, RectDirection? hoveredDirection)
        {
            var brush = BorderBrush ?? Brushes.DodgerBlue;
            double cellSize = DirectionHostSize / 3.0;

            // 根据当前方向计算容器中心点
            Point hostCenter;
            switch (direction)
            {
                case RectDirection.Top:
                    hostCenter = new Point(w / 2, -DirectionHostOffset + DirectionHostSize / 2);
                    break;

                case RectDirection.Bottom:
                    hostCenter = new Point(w / 2, h + DirectionHostOffset - DirectionHostSize / 2);
                    break;

                case RectDirection.Left:
                    hostCenter = new Point(-DirectionHostOffset + DirectionHostSize / 2, h / 2);
                    break;

                case RectDirection.Right:
                    hostCenter = new Point(w + DirectionHostOffset - DirectionHostSize / 2, h / 2);
                    break;

                default:
                    hostCenter = new Point(w / 2, -DirectionHostOffset + DirectionHostSize / 2);
                    break;
            }

            double left = hostCenter.X - DirectionHostSize / 2;
            double top = hostCenter.Y - DirectionHostSize / 2;

            // 上方三角形按钮
            if (direction != RectDirection.Top)
            {
                DrawSmallTriangle(dc, brush,
                    new Point(left + cellSize * 1.5, top + cellSize * 0.5), 0,
                    hoveredDirection == RectDirection.Top);
            }

            // 左侧三角形按钮
            if (direction != RectDirection.Left)
            {
                DrawSmallTriangle(dc, brush,
                    new Point(left + cellSize * 0.5, top + cellSize * 1.5), -90,
                    hoveredDirection == RectDirection.Left);
            }

            // 右侧三角形按钮
            if (direction != RectDirection.Right)
            {
                DrawSmallTriangle(dc, brush,
                    new Point(left + cellSize * 2.5, top + cellSize * 1.5), 90,
                    hoveredDirection == RectDirection.Right);
            }

            // 下方三角形按钮
            if (direction != RectDirection.Bottom)
            {
                DrawSmallTriangle(dc, brush,
                    new Point(left + cellSize * 1.5, top + cellSize * 2.5), 180,
                    hoveredDirection == RectDirection.Bottom);
            }
        }


        /// <summary>
        /// 绘制一个小巧的实心等边三角形，并进行指定角度的旋转。
        /// </summary>
        private void DrawSmallTriangle(DrawingContext dc, Brush brush, Point center, double angleDeg, bool isHovered = false)
        {
            double halfBase = DirectionTriSize / 2.0;
            double height = DirectionTriSize * 0.866;

            // 基础三角形向上
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
                    center.X + p.X * cos - p.Y * sin,
                    center.Y + p.X * sin + p.Y * cos);
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

            // 如果处于悬停状态，则绘制对比色的边框
            Pen strokePen = isHovered ? HoverStrokePen : null;
            dc.DrawGeometry(brush, strokePen, geometry);
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
        public TransformOperation HitTest(Point point, Size size, RectDirection currentDirection)
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
                return TransformOperation.ResizeTopLeft;
            }
            if (IsPointInRect(point, tr, ResizeHitSize))
            {
                return TransformOperation.ResizeTopRight;
            }
            if (IsPointInRect(point, br, ResizeHitSize))
            {
                return TransformOperation.ResizeBottomRight;
            }
            if (IsPointInRect(point, bl, ResizeHitSize))
            {
                return TransformOperation.ResizeBottomLeft;
            }

            // 2. 检查旋转角点（次要优先级）
            if (IsPointInRect(point, tl, RotateHitSize))
            {
                return TransformOperation.RotateTopLeft;
            }
            if (IsPointInRect(point, tr, RotateHitSize))
            {
                return TransformOperation.RotateTopRight;
            }
            if (IsPointInRect(point, br, RotateHitSize))
            {
                return TransformOperation.RotateBottomRight;
            }
            if (IsPointInRect(point, bl, RotateHitSize))
            {
                return TransformOperation.RotateBottomLeft;
            }

            // 3. 检查缩放边缘
            if (Math.Abs(point.Y) <= ResizeHitSize / 2 && point.X > 0 && point.X < w)
            {
                return TransformOperation.ResizeTop;
            }
            if (Math.Abs(point.Y - h) <= ResizeHitSize / 2 && point.X > 0 && point.X < w)
            {
                return TransformOperation.ResizeBottom;
            }
            if (Math.Abs(point.X) <= ResizeHitSize / 2 && point.Y > 0 && point.Y < h)
            {
                return TransformOperation.ResizeLeft;
            }
            if (Math.Abs(point.X - w) <= ResizeHitSize / 2 && point.Y > 0 && point.Y < h)
            {
                return TransformOperation.ResizeRight;
            }

            // 4. 检查主方向箭头（用于旋转）
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
                return TransformOperation.Rotate;
            }

            // 5. 检查主体内部（用于移动）
            if (point.X >= 0 && point.X <= w && point.Y >= 0 && point.Y <= h)
            {
                return TransformOperation.Move;
            }

            return TransformOperation.None;
        }

        private static bool IsPointInRect(Point p, Point center, double size)
        {
            return p.X >= center.X - size / 2 && p.X <= center.X + size / 2 &&
                   p.Y >= center.Y - size / 2 && p.Y <= center.Y + size / 2;
        }


        /// <summary>
        /// 针对 4 个方向的按钮进行命中测试。
        /// 返回命中的方向，如果未命中则返回当前的方向。
        /// </summary>
        public RectDirection HitTestDirectionButton(Point point, Size size, RectDirection currentDirection)
        {
            double w = size.Width;
            double h = size.Height;
            double cellSize = DirectionHostSize / 3.0;
            double hitRadius = cellSize;

            // 计算容器中心点
            Point hostCenter;
            switch (currentDirection)
            {
                case RectDirection.Top:
                    hostCenter = new Point(w / 2, -DirectionHostOffset + DirectionHostSize / 2);
                    break;

                case RectDirection.Bottom:
                    hostCenter = new Point(w / 2, h + DirectionHostOffset - DirectionHostSize / 2);
                    break;

                case RectDirection.Left:
                    hostCenter = new Point(-DirectionHostOffset + DirectionHostSize / 2, h / 2);
                    break;

                case RectDirection.Right:
                    hostCenter = new Point(w + DirectionHostOffset - DirectionHostSize / 2, h / 2);
                    break;

                default:
                    hostCenter = new Point(w / 2, -DirectionHostOffset + DirectionHostSize / 2);
                    break;
            }

            double left = hostCenter.X - DirectionHostSize / 2;
            double top = hostCenter.Y - DirectionHostSize / 2;

            // 测试上方按钮
            if (currentDirection != RectDirection.Top)
            {
                Point center = new Point(left + cellSize * 1.5, top + cellSize * 0.5);
                if (IsPointInRect(point, center, hitRadius))
                {
                    return RectDirection.Top;
                }
            }

            // 测试左侧按钮
            if (currentDirection != RectDirection.Left)
            {
                Point center = new Point(left + cellSize * 0.5, top + cellSize * 1.5);
                if (IsPointInRect(point, center, hitRadius))
                {
                    return RectDirection.Left;
                }
            }

            // 测试右侧按钮
            if (currentDirection != RectDirection.Right)
            {
                Point center = new Point(left + cellSize * 2.5, top + cellSize * 1.5);
                if (IsPointInRect(point, center, hitRadius))
                {
                    return RectDirection.Right;
                }
            }

            // 测试下方按钮
            if (currentDirection != RectDirection.Bottom)
            {
                Point center = new Point(left + cellSize * 1.5, top + cellSize * 2.5);
                if (IsPointInRect(point, center, hitRadius))
                {
                    return RectDirection.Bottom;
                }
            }

            return currentDirection;
        }

        #endregion
    }
}
