using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace RS.Widgets.Converters
{
    public class ClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3 ||
                !(values[0] is CornerRadius borderCornerRadius) ||
                !(values[1] is double width) ||
                !(values[2] is double height))
            {
                return null;
            }

            if (width <= 0 || height <= 0)
            {
                return null;
            }
         
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure
            {
                StartPoint = new Point(borderCornerRadius.TopLeft, 0)
            };
            pathFigure.Segments.Add(new LineSegment(new Point(width - borderCornerRadius.TopRight, 0), true));
            pathFigure.Segments.Add(new ArcSegment(
                new Point(width, borderCornerRadius.TopRight),
                new Size(borderCornerRadius.TopRight, borderCornerRadius.TopRight),
                0, false, SweepDirection.Clockwise, true));
            pathFigure.Segments.Add(new LineSegment(new Point(width, height - borderCornerRadius.BottomRight), true));
            pathFigure.Segments.Add(new ArcSegment(
                new Point(width - borderCornerRadius.BottomRight, height),
                new Size(borderCornerRadius.BottomRight, borderCornerRadius.BottomRight),
                0, false, SweepDirection.Clockwise, true));
            pathFigure.Segments.Add(new LineSegment(new Point(borderCornerRadius.BottomLeft, height), true));
            pathFigure.Segments.Add(new ArcSegment(
                new Point(0, height - borderCornerRadius.BottomLeft),
                new Size(borderCornerRadius.BottomLeft, borderCornerRadius.BottomLeft),
                0, false, SweepDirection.Clockwise, true));
            pathFigure.Segments.Add(new LineSegment(new Point(0, borderCornerRadius.TopLeft), true));
            pathFigure.Segments.Add(new ArcSegment(
                new Point(borderCornerRadius.TopLeft, 0),
                new Size(borderCornerRadius.TopLeft, borderCornerRadius.TopLeft),
                0, false, SweepDirection.Clockwise, true));
            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
