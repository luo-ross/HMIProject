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
        /// 根据容器长度和宽度还有圆角大小设置裁剪
        /// </summary>
        /// <param name="values">参数1是圆角 参数2是宽度 参数3是高度</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
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

            // 创建 PathGeometry
            PathGeometry pathGeometry = new PathGeometry();
            // 创建 PathFigure
            PathFigure pathFigure = new PathFigure
            {
                StartPoint = new Point(borderCornerRadius.TopLeft, 0) // 起点
            };

            // 添加顶部边缘
            pathFigure.Segments.Add(new LineSegment(new Point(width - borderCornerRadius.TopRight, 0), true));

            // 添加右上角圆角（半径 20）
            pathFigure.Segments.Add(new ArcSegment(
                new Point(width, borderCornerRadius.TopRight), // 终点
                new Size(borderCornerRadius.TopRight, borderCornerRadius.TopRight),   // 半径
                0,                  // 旋转角度
                false,              // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));

            // 添加右侧边缘
            pathFigure.Segments.Add(new LineSegment(new Point(width, height - borderCornerRadius.BottomRight), true));

            // 添加右下角圆角（半径 40）
            pathFigure.Segments.Add(new ArcSegment(
                new Point(width - borderCornerRadius.BottomRight, height), // 终点
                new Size(borderCornerRadius.BottomRight, borderCornerRadius.BottomRight),    // 半径
                0,                   // 旋转角度
                false,               // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));
            // 添加底部边缘
            pathFigure.Segments.Add(new LineSegment(new Point(borderCornerRadius.BottomLeft, height), true));
            // 添加左下角圆角（半径 30）
            pathFigure.Segments.Add(new ArcSegment(
                new Point(0, height - borderCornerRadius.BottomLeft), // 终点
                new Size(borderCornerRadius.BottomLeft, borderCornerRadius.BottomLeft),   // 半径
                0,                  // 旋转角度
                false,              // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));
            // 添加左侧边缘
            pathFigure.Segments.Add(new LineSegment(new Point(0, borderCornerRadius.TopLeft), true));
            // 添加左上角圆角（半径 10）
            pathFigure.Segments.Add(new ArcSegment(
                new Point(borderCornerRadius.TopLeft, 0), // 终点
                new Size(borderCornerRadius.TopLeft, borderCornerRadius.TopLeft), // 半径
                0,                // 旋转角度
                false,            // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));
            // 将 PathFigure 添加到 PathGeometry
            pathGeometry.Figures.Add(pathFigure);
            return pathGeometry;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
