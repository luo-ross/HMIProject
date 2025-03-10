using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class RSCarouselSlider : Thumb
    {
        static RSCarouselSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSCarouselSlider), new FrameworkPropertyMetadata(typeof(RSCarouselSlider)));
        }


        public int Id { get; set; }


        [Description("图像资源绝对路径")]
        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(RSCarouselSlider), new PropertyMetadata(null));



        [Description("标题")]
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(RSCarouselSlider), new PropertyMetadata(null));



        [Description("描述")]
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(RSCarouselSlider), new PropertyMetadata(null));


        [Description("地点")]
        public string Location
        {
            get { return (string)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(string), typeof(RSCarouselSlider), new PropertyMetadata(null));


        [Description("模糊度")]
        public double BlurRadius
        {
            get { return (double)GetValue(BlurRadiusProperty); }
            set { SetValue(BlurRadiusProperty, value); }
        }

        public static readonly DependencyProperty BlurRadiusProperty =
            DependencyProperty.Register("BlurRadius", typeof(double), typeof(RSCarouselSlider), new PropertyMetadata(0D));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
