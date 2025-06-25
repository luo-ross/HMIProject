using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace RS.Widgets.Converters
{
    // <summary>
    /// 根据原始颜色和 BlendRatio，将背景色与白色线性混合，模拟 Opacity 效果但不透明。
    /// 有背景色时，BlendRatio=0 返回原色，BlendRatio>0 时与白色混合。
    /// 无背景色时，BlendRatio=0 返回null或Transparent，BlendRatio>0 时用BaseColorWhenTransparent（默认#FFDEDEDE）与白色混合。
    /// </summary>
    public class ColorBlendConverter : IMultiValueConverter
    {
        /// <summary>
        /// 无背景色时的基准色，默认#FFDEDEDE
        /// </summary>
        public Color BaseColorWhenTransparent { get; set; } = (Color)ColorConverter.ConvertFromString("#FFDEDEDE");

        /// <summary>
        /// 调和目标色，默认白色，可通过属性或 ConverterParameter 动态配置
        /// </summary>
        public Color BlendTarget { get; set; } = Colors.White;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
            {
                return values[0];
            }

            // 1. 获取基础色
            Color? baseColor = null;
            if (values[0] is SolidColorBrush brush)
            {
                baseColor = brush.Color;
            }
            else if (values[0] is Color color)
            {
                baseColor = color;
            }

            // 2. 获取BlendRatio
            double ratio = 0.0;
            if (values[1] is double d)
            {
                ratio = d;
            }
            else if (values[1] is string s && double.TryParse(s, out double d2))
            {
                ratio = d2;
            }
            ratio = Math.Max(0, Math.Min(1, ratio));

            // 支持通过 ConverterParameter 动态传递 BlendTarget
            Color blendTarget = BlendTarget;
            if (parameter is Color paramColor)
            {
                blendTarget = paramColor;
            }
            else if (parameter is SolidColorBrush paramBrush)
            {
                blendTarget = paramBrush.Color;
            }
            else if (parameter is string paramStr)
            {
                try { blendTarget = (Color)ColorConverter.ConvertFromString(paramStr); } catch { }
            }

            // 3. 处理逻辑
            if (baseColor == null || baseColor.Value.A == 0)
            {
                if (ratio == 0)
                {
                    // 无背景色且BlendRatio=0，返回null或Transparent
                    return values[0];
                }
                else
                {
                    return new SolidColorBrush(MixColor(BaseColorWhenTransparent, blendTarget, ratio));
                }
            }
            else
            {
                if (ratio == 0)
                {
                    // 有背景色且BlendRatio=0，直接返回原色
                    return new SolidColorBrush(baseColor.Value);
                }
                else
                {
                    return new SolidColorBrush(MixColor(baseColor.Value, blendTarget, ratio));
                }
            }
        }

        private static Color MixColor(Color from, Color to, double ratio)
        {
            byte r = (byte)(from.R * (1 - ratio) + to.R * ratio);
            byte g = (byte)(from.G * (1 - ratio) + to.G * ratio);
            byte b = (byte)(from.B * (1 - ratio) + to.B * ratio);
            return Color.FromArgb(255, r, g, b);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
