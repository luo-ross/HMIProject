using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RS.Widgets.Extensions
{
    public static class ScottPlotColorExtension
    {
        public static ScottPlot.Color ToScottPlotColor(this Color color)
        {
            return new ScottPlot.Color(color.R, color.G, color.B, color.A);
        }

        public static ScottPlot.Color ToScottPlotColor(this Brush brush)
        {
            if (brush == null)
                return ScottPlot.Colors.Transparent;

            // SolidColorBrush 直接获取颜色
            if (brush is SolidColorBrush solidColorBrush)
            {
                return solidColorBrush.Color.ToScottPlotColor();
            }

            // GradientBrush 获取第一个 GradientStop 的颜色
            if (brush is GradientBrush gradientBrush && gradientBrush.GradientStops.Count > 0)
            {
                return gradientBrush.GradientStops[0].Color.ToScottPlotColor(); 
            }

            // 其他类型的 Brush（如 ImageBrush、VisualBrush）无法直接转换为单一颜色
            return ScottPlot.Colors.Transparent;
        }
    }
}
