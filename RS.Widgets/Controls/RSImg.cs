using RS.Commons;
using RS.Widgets.Commons;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class RSImg : CheckBox
    {
        static RSImg()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSImg), new FrameworkPropertyMetadata(typeof(RSImg)));
        }


        public ImgModel ImgModel
        {
            get { return (ImgModel)GetValue(ImgModelProperty); }
            set { SetValue(ImgModelProperty, value); }
        }

        public static readonly DependencyProperty ImgModelProperty =
            DependencyProperty.Register("ImgModel", typeof(ImgModel), typeof(RSImg), new PropertyMetadata(default));





        public double Brightness
        {
            get { return (double)GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        public static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register("Brightness", typeof(double), typeof(RSImg), new PropertyMetadata(0D));




        public double Contrast
        {
            get { return (double)GetValue(ContrastProperty); }
            set { SetValue(ContrastProperty, value); }
        }

        public static readonly DependencyProperty ContrastProperty =
            DependencyProperty.Register("Contrast", typeof(double), typeof(RSImg), new PropertyMetadata(0D));




        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
