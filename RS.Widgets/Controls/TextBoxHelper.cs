using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public static class TextBoxHelper
    {
      
        /// <summary>
        /// 文本框输入水印
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(TextBoxHelper), new PropertyMetadata("请输入内容..."));

        public static string GetWatermark(DependencyObject obj)
        {
            return (string)obj.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject obj, string value)
        {
            obj.SetValue(WatermarkProperty, value);
        }


        /// <summary>
        /// 是否显示清除按钮
        /// </summary>
        public static readonly DependencyProperty IsShowClearBtnProperty =
            DependencyProperty.RegisterAttached("IsShowClearBtn", typeof(bool), typeof(TextBoxHelper), new PropertyMetadata(false));

        public static bool GetIsShowClearBtn(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsShowClearBtnProperty);
        }

        public static void SetIsShowClearBtn(DependencyObject obj, bool value)
        {
            obj.SetValue(IsShowClearBtnProperty, value);
        }
    }

}
