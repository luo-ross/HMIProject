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

        /// <summary>
        /// 根据容器长度、宽度、圆角大小和边框厚度设置裁剪。
        /// </summary>
        /// <param name="values">应包含4个值: [0]CornerRadius, [1]Width, [2]Height, [3]BorderThickness</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4 ||
                !(values[0] is CornerRadius cornerRadius) ||
                !(values[1] is double width) ||
                !(values[2] is double height) ||
                !(values[3] is Thickness borderThickness))
            {
                return Geometry.Empty;
            }
            var pathGeometry = GetPathGeometry(
              cornerRadius,
              width,
              height,
              borderThickness);
            return pathGeometry;
        }

        public static Geometry GetPathGeometry(
            CornerRadius cornerRadius,
            double width,
            double height,
            Thickness borderThickness
            )
        {

            if (width <= 0 || height <= 0)
            {
                return Geometry.Empty;
            }

            // 计算一个向内收缩的矩形区域
            double left = borderThickness.Left / 2;
            double top = borderThickness.Top / 2;
            double right = width - borderThickness.Right / 2;
            double bottom = height - borderThickness.Bottom / 2;

            // 确保计算后的尺寸不会小于0
            if (right < left || bottom < top)
            {
                return Geometry.Empty;
            }

            double topLeftRadius = Math.Min(cornerRadius.TopLeft, (right - left) / 2);
            topLeftRadius = Math.Min(topLeftRadius, (bottom - top) / 2);

            double topRightRadius = Math.Min(cornerRadius.TopRight, (right - left) / 2);
            topRightRadius = Math.Min(topRightRadius, (bottom - top) / 2);

            double bottomRightRadius = Math.Min(cornerRadius.BottomRight, (right - left) / 2);
            bottomRightRadius = Math.Min(bottomRightRadius, (bottom - top) / 2);

            double bottomLeftRadius = Math.Min(cornerRadius.BottomLeft, (right - left) / 2);
            bottomLeftRadius = Math.Min(bottomLeftRadius, (bottom - top) / 2);


            // 创建 PathGeometry
            PathGeometry pathGeometry = new PathGeometry();
            // 创建 PathFigure
            PathFigure pathFigure = new PathFigure
            {
                //更新所有点的坐标，基于新的 left, top, right, bottom
                StartPoint = new Point(left + topLeftRadius, top) // 起点
            };

            // 添加顶部边缘
            pathFigure.Segments.Add(new LineSegment(new Point(right - topRightRadius, top), true));

            // 添加右上角圆角
            pathFigure.Segments.Add(new ArcSegment(
                new Point(right, top + topRightRadius), // 终点
                new Size(topRightRadius, topRightRadius),   // 半径
                0,                  // 旋转角度
                false,              // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));

            // 添加右侧边缘
            pathFigure.Segments.Add(new LineSegment(new Point(right, bottom - bottomRightRadius), true));

            // 添加右下角圆角
            pathFigure.Segments.Add(new ArcSegment(
                new Point(right - bottomRightRadius, bottom), // 终点
                new Size(bottomRightRadius, bottomRightRadius),    // 半径
                0,                   // 旋转角度
                false,               // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));
            // 添加底部边缘
            pathFigure.Segments.Add(new LineSegment(new Point(left + bottomLeftRadius, bottom), true));
            // 添加左下角圆角
            pathFigure.Segments.Add(new ArcSegment(
                new Point(left, bottom - bottomLeftRadius), // 终点
                new Size(bottomLeftRadius, bottomLeftRadius),   // 半径
                0,                  // 旋转角度
                false,              // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));
            // 添加左侧边缘
            pathFigure.Segments.Add(new LineSegment(new Point(left, top + topLeftRadius), true));
            // 添加左上角圆角
            pathFigure.Segments.Add(new ArcSegment(
                new Point(left + topLeftRadius, top), // 终点
                new Size(topLeftRadius, topLeftRadius), // 半径
                0,                // 旋转角度
                false,            // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));
            // 将 PathFigure 添加到 PathGeometry
            pathGeometry.Figures.Add(pathFigure);

            // 冻结以提高性能
            pathGeometry.Freeze();

            return pathGeometry;
        }



        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
