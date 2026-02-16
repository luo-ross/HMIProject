using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RS.Widgets.Converters
{
    /// <summary>
    /// Brush颜色加深转换器：接收Brush，返回更深的对比色Brush
    /// </summary>
    [ValueConversion(typeof(Brush), typeof(Brush))]
    public class DarkerContrastBrushConverter : IValueConverter
    {
        #region 可配置参数（通过XAML绑定/设置）
        /// <summary>
        /// 加深程度（0~1，默认0.4，值越大颜色越深）
        /// </summary>
        public double DarkenLevel { get; set; } = 0.4;

        /// <summary>
        /// 最小亮度阈值（防止颜色过黑，默认0.1）
        /// </summary>
        public double MinBrightness { get; set; } = 0.1;
        #endregion

        /// <summary>
        /// 正向转换：Brush → 更深的对比色Brush
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 1. 空值/非Brush处理
            if (value == null || value == DependencyProperty.UnsetValue || !(value is Brush inputBrush))
            {
                // 返回默认深灰色（兜底）
                return Brushes.DarkGray;
            }

            try
            {
                // 2. 提取核心颜色（兼容SolidColorBrush/渐变Brush/其他Brush）
                Color baseColor = GetCoreColorFromBrush(inputBrush);

                // 3. 计算更深的对比色
                Color darkerColor = GetDarkerContrastColor(baseColor, DarkenLevel, MinBrightness);

                // 4. 修正：先创建实例，再调用Freeze()方法（核心修改处）
                var darkerBrush = new SolidColorBrush(darkerColor);
                // 冻结Brush提升性能（只读，不可修改）
                if (!darkerBrush.IsFrozen)
                {
                    darkerBrush.Freeze();
                }
                return darkerBrush;
            }
            catch (Exception ex)
            {
                // 异常兜底：返回深灰色，避免UI崩溃
                System.Diagnostics.Debug.WriteLine($"颜色加深转换失败：{ex.Message}");
                return Brushes.DarkGray;
            }
        }

        /// <summary>
        /// 反向转换（不支持，返回原值）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("DarkerContrastBrushConverter不支持反向转换");
        }

        #region 核心辅助方法
        /// <summary>
        /// 从任意Brush中提取核心颜色
        /// </summary>
        private Color GetCoreColorFromBrush(Brush brush)
        {
            // 情况1：纯色Brush（最常见）
            if (brush is SolidColorBrush solidBrush)
            {
                return solidBrush.Color;
            }

            // 情况2：渐变Brush（取第一个渐变停止点的颜色）
            if (brush is LinearGradientBrush linearGradient)
            {
                return linearGradient.GradientStops.Count > 0
                    ? linearGradient.GradientStops[0].Color
                    : Colors.Gray;
            }
            if (brush is RadialGradientBrush radialGradient)
            {
                return radialGradient.GradientStops.Count > 0
                    ? radialGradient.GradientStops[0].Color
                    : Colors.Gray;
            }

            // 情况3：其他Brush（取默认颜色）
            return Colors.Gray;
        }

        /// <summary>
        /// 计算更深的对比色（基于HSB色彩空间，更符合人眼感知）
        /// </summary>
        private Color GetDarkerContrastColor(Color baseColor, double darkenLevel, double minBrightness)
        {
            // 1. RGB转HSB（H:色相, S:饱和度, B:亮度）
            ColorToHSB(baseColor, out double h, out double s, out double b);

            // 2. 调整亮度和饱和度（降低亮度，提高饱和度，生成更深的对比色）
            double newBrightness = Math.Max(b * (1 - darkenLevel), minBrightness); // 降低亮度，不低于最小阈值
            double newSaturation = Math.Min(s * (1 + darkenLevel / 2), 1.0); // 提高饱和度，不超过1

            // 3. HSB转回RGB
            return HSBToColor(h, newSaturation, newBrightness);
        }

        /// <summary>
        /// RGB转HSB（色相[0-360], 饱和度[0-1], 亮度[0-1]）
        /// </summary>
        private void ColorToHSB(Color color, out double h, out double s, out double b)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double bVal = color.B / 255.0;

            double max = Math.Max(Math.Max(r, g), bVal);
            double min = Math.Min(Math.Min(r, g), bVal);
            double delta = max - min;

            // 计算色相H
            if (delta == 0)
                h = 0;
            else if (max == r)
                h = (60 * ((g - bVal) / delta) + 360) % 360;
            else if (max == g)
                h = (60 * ((bVal - r) / delta) + 120) % 360;
            else
                h = (60 * ((r - g) / delta) + 240) % 360;

            // 计算饱和度S
            s = max == 0 ? 0 : delta / max;

            // 计算亮度B
            b = max;
        }

        /// <summary>
        /// HSB转RGB
        /// </summary>
        private Color HSBToColor(double h, double s, double b)
        {
            if (s == 0)
            {
                // 灰度色
                byte val = (byte)(b * 255);
                return Color.FromRgb(val, val, val);
            }

            h = h % 360;
            double c = b * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = b - c;

            double r, g, bVal;
            if (h < 60)
            {
                r = c; g = x; bVal = 0;
            }
            else if (h < 120)
            {
                r = x; g = c; bVal = 0;
            }
            else if (h < 180)
            {
                r = 0; g = c; bVal = x;
            }
            else if (h < 240)
            {
                r = 0; g = x; bVal = c;
            }
            else if (h < 300)
            {
                r = x; g = 0; bVal = c;
            }
            else
            {
                r = c; g = 0; bVal = x;
            }

            byte red = (byte)((r + m) * 255);
            byte green = (byte)((g + m) * 255);
            byte blue = (byte)((bVal + m) * 255);

            return Color.FromRgb(red, green, blue);
        }
        #endregion
    }
}
