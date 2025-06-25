using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace RS.Widgets.Structs
{
    public struct CornerRadius2D : IEquatable<CornerRadius2D>
    {
        public double TopLeftX { get; set; }
        public double TopLeftY { get; set; }
        public double TopRightX { get; set; }
        public double TopRightY { get; set; }
        public double BottomLeftX { get; set; }
        public double BottomLeftY { get; set; }
        public double BottomRightX { get; set; }
        public double BottomRightY { get; set; }

        public CornerRadius2D(
            double topLeftX, double topLeftY,
            double topRightX, double topRightY,
            double bottomRightX, double bottomRightY,
            double bottomLeftX, double bottomLeftY)
            : this()
        {
            TopLeftX = topLeftX;
            TopLeftY = topLeftY;
            TopRightX = topRightX;
            TopRightY = topRightY;
            BottomRightX = bottomRightX;
            BottomRightY = bottomRightY;
            BottomLeftX = bottomLeftX;
            BottomLeftY = bottomLeftY;
        }

        public CornerRadius2D(CornerRadius cornerRadius)
            : this(
                cornerRadius.TopLeft, cornerRadius.TopLeft,
                cornerRadius.TopRight, cornerRadius.TopRight,
                cornerRadius.BottomRight, cornerRadius.BottomRight,
                cornerRadius.BottomLeft, cornerRadius.BottomLeft)
        { }

        public CornerRadius2D(double uniformRadius)
            : this(uniformRadius, uniformRadius, uniformRadius, uniformRadius, uniformRadius, uniformRadius, uniformRadius, uniformRadius)
        { }

        public static implicit operator CornerRadius2D(CornerRadius cornerRadius)
        {
            return new CornerRadius2D(cornerRadius);
        }

        public override string ToString()
        {
            return $"TL({TopLeftX},{TopLeftY}) TR({TopRightX},{TopRightY}) BR({BottomRightX},{BottomRightY}) BL({BottomLeftX},{BottomLeftY})";
        }

        public CornerRadius ToCornerRadius()
        {
            return new CornerRadius(
                (TopLeftX + TopLeftY) / 2,
                (TopRightX + TopRightY) / 2,
                (BottomRightX + BottomRightY) / 2,
                (BottomLeftX + BottomLeftY) / 2
            );
        }

        public static StreamGeometry CreateRoundedRectangleGeometry(Rect rect, CornerRadius2D radius)
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(rect.Left + radius.TopLeftX, rect.Top), true, true);

                // Top edge
                context.LineTo(new Point(rect.Right - radius.TopRightX, rect.Top), true, false);
                context.ArcTo(new Point(rect.Right, rect.Top + radius.TopRightY),
                    new Size(radius.TopRightX, radius.TopRightY), 0, false, SweepDirection.Clockwise, true, false);

                // Right edge
                context.LineTo(new Point(rect.Right, rect.Bottom - radius.BottomRightY), true, false);
                context.ArcTo(new Point(rect.Right - radius.BottomRightX, rect.Bottom),
                    new Size(radius.BottomRightX, radius.BottomRightY), 0, false, SweepDirection.Clockwise, true, false);

                // Bottom edge
                context.LineTo(new Point(rect.Left + radius.BottomLeftX, rect.Bottom), true, false);
                context.ArcTo(new Point(rect.Left, rect.Bottom - radius.BottomLeftY),
                    new Size(radius.BottomLeftX, radius.BottomLeftY), 0, false, SweepDirection.Clockwise, true, false);

                // Left edge
                context.LineTo(new Point(rect.Left, rect.Top + radius.TopLeftY), true, false);
                context.ArcTo(new Point(rect.Left + radius.TopLeftX, rect.Top),
                    new Size(radius.TopLeftX, radius.TopLeftY), 0, false, SweepDirection.Clockwise, true, false);
            }
            geometry.Freeze();
            return geometry;
        }

        public override bool Equals(object? obj)
        {
            if (obj is CornerRadius2D)
            {
                return Equals((CornerRadius2D)obj);
            }
            return false;
        }

        public bool Equals(CornerRadius2D other)
        {
            return TopLeftX.Equals(other.TopLeftX) && TopLeftY.Equals(other.TopLeftY) &&
                   TopRightX.Equals(other.TopRightX) && TopRightY.Equals(other.TopRightY) &&
                   BottomRightX.Equals(other.BottomRightX) && BottomRightY.Equals(other.BottomRightY) &&
                   BottomLeftX.Equals(other.BottomLeftX) && BottomLeftY.Equals(other.BottomLeftY);
        }

        public static bool operator ==(CornerRadius2D left, CornerRadius2D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CornerRadius2D left, CornerRadius2D right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + TopLeftX.GetHashCode();
                hash = hash * 23 + TopLeftY.GetHashCode();
                hash = hash * 23 + TopRightX.GetHashCode();
                hash = hash * 23 + TopRightY.GetHashCode();
                hash = hash * 23 + BottomRightX.GetHashCode();
                hash = hash * 23 + BottomRightY.GetHashCode();
                hash = hash * 23 + BottomLeftX.GetHashCode();
                hash = hash * 23 + BottomLeftY.GetHashCode();
                return hash;
            }
        }

        public void Scale(double scale)
        {
            TopLeftX *= scale;
            TopLeftY *= scale;
            TopRightX *= scale;
            TopRightY *= scale;
            BottomRightX *= scale;
            BottomRightY *= scale;
            BottomLeftX *= scale;
            BottomLeftY *= scale;
        }
    }
}
