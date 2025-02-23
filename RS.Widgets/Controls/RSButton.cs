using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class RSButton : Button
    {
        static RSButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSButton), new FrameworkPropertyMetadata(typeof(RSButton)));
        }

        [Browsable(true)]
        [Category("自定义样式设置")]
        [Description("圆角边框设置")]
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(RSButton), new PropertyMetadata(default(CornerRadius)));



        [Browsable(true)]
        [Category("自定义样式设置")]
        [Description("按钮类型设置")]
        public BtnType BtnType
        {
            get { return (BtnType)GetValue(BtnTypeProperty); }
            set { SetValue(BtnTypeProperty, value); }
        }
        public static readonly DependencyProperty BtnTypeProperty =
            DependencyProperty.Register("BtnType", typeof(BtnType), typeof(RSButton), new PropertyMetadata(BtnType.Standard));


        [Browsable(true)]
        [Category("自定义样式设置")]
        [Description("按钮颜色类型")]
        public BtnColorType BtnColorType
        {
            get { return (BtnColorType)GetValue(BtnColorTypeProperty); }
            set { SetValue(BtnColorTypeProperty, value); }
        }

        public static readonly DependencyProperty BtnColorTypeProperty =
            DependencyProperty.Register("BtnColorType", typeof(BtnColorType), typeof(RSButton), new PropertyMetadata(BtnColorType.Primary));


        [Browsable(true)]
        [Category("自定义样式设置")]
        [Description("Icon图标")]
        public Geometry IconData
        {
            get { return (Geometry)GetValue(IconDataProperty); }
            set { SetValue(IconDataProperty, value); }
        }
        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.Register("IconData", typeof(Geometry), typeof(RSButton), new PropertyMetadata(null));



        [Browsable(true)]
        [Category("自定义样式设置")]
        [Description("Icon图标宽度")]
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(RSButton), new PropertyMetadata(15D));



        [Browsable(true)]
        [Category("自定义样式设置")]
        [Description("Icon图标高度")]
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(RSButton), new PropertyMetadata(15D));




        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
