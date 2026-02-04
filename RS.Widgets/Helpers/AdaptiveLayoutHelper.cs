using System.Windows;

namespace RS.Widgets.Helpers
{
    /// <summary>
    /// 自适应布局助手类，通过附加属性提供计算后的布局状态
    /// </summary>
    public static class AdaptiveLayoutHelper
    {
        #region Attached Inputs

        public static readonly DependencyProperty IsPreviewCheckedProperty =
            DependencyProperty.RegisterAttached("IsPreviewChecked", typeof(bool), typeof(AdaptiveLayoutHelper), 
                new PropertyMetadata(false, OnInputChanged));

        public static bool GetIsPreviewChecked(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPreviewCheckedProperty);
        }

        public static void SetIsPreviewChecked(DependencyObject obj, bool value)
        {
            obj.SetValue(IsPreviewCheckedProperty, value);
        }

        public static readonly DependencyProperty TotalWidthProperty =
            DependencyProperty.RegisterAttached("TotalWidth", typeof(double), typeof(AdaptiveLayoutHelper), 
                new PropertyMetadata(0.0, OnInputChanged));

        public static double GetTotalWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(TotalWidthProperty);
        }

        public static void SetTotalWidth(DependencyObject obj, double value)
        {
            obj.SetValue(TotalWidthProperty, value);
        }

        public static readonly DependencyProperty ContainerWidthProperty =
            DependencyProperty.RegisterAttached("ContainerWidth", typeof(double), typeof(AdaptiveLayoutHelper), 
                new PropertyMetadata(0.0, OnInputChanged));

        public static double GetContainerWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(ContainerWidthProperty);
        }

        public static void SetContainerWidth(DependencyObject obj, double value)
        {
            obj.SetValue(ContainerWidthProperty, value);
        }

        #endregion

        #region Attached Outputs (Calculated)

        public static readonly DependencyProperty ShowPreviewPaneProperty =
            DependencyProperty.RegisterAttached("ShowPreviewPane", typeof(bool), typeof(AdaptiveLayoutHelper), 
                new PropertyMetadata(false));

        public static bool GetShowPreviewPane(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowPreviewPaneProperty);
        }

        public static void SetShowPreviewPane(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowPreviewPaneProperty, value);
        }

        public static readonly DependencyProperty UseCompactTemplateProperty =
            DependencyProperty.RegisterAttached("UseCompactTemplate", typeof(bool), typeof(AdaptiveLayoutHelper), 
                new PropertyMetadata(false));

        public static bool GetUseCompactTemplate(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseCompactTemplateProperty);
        }

        public static void SetUseCompactTemplate(DependencyObject obj, bool value)
        {
            obj.SetValue(UseCompactTemplateProperty, value);
        }

        #endregion

        private static void OnInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateCalculatedStates(d);
        }

        private static void UpdateCalculatedStates(DependencyObject d)
        {
            bool isPreviewChecked = GetIsPreviewChecked(d);
            double totalWidth = GetTotalWidth(d);
            double containerWidth = GetContainerWidth(d);

            // 是否开启预览：开关打开 且 总宽度足够 (> 825)
            SetShowPreviewPane(d, isPreviewChecked && totalWidth > 825);

            // 是否使用紧凑模板：容器宽度不足 (< 745)
            SetUseCompactTemplate(d, containerWidth < 745);
        }
    }
}
