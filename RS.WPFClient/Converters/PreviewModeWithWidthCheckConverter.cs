using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RS.WPFClient.Converters
{
    /// <summary>
    /// 预览模式宽度检查转换器
    /// 当预览模式开启且宽度小于指定阈值时返回true
    /// </summary>
    public class PreviewModeWithWidthCheckConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return false;
            }

            // values[0]: IsPreviewMode (bool)
            // values[1]: ActualWidth (double)
            if (values[0] is bool isPreviewMode && values[1] is double actualWidth)
            {
                double threshold = 750.0;
                
                // 如果提供了参数，使用参数作为阈值
                if (parameter != null && double.TryParse(parameter.ToString(), out double customThreshold))
                {
                    threshold = customThreshold;
                }

                // 预览模式为true 且 实际宽度小于阈值时返回true
                return isPreviewMode && actualWidth < threshold;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
