using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RS.Annotation.Widgets.Controls
{
    /// <summary>
    /// RSImg.xaml 的交互逻辑
    /// </summary>
    public partial class RSImg : CheckBox
    {
        public RSImg()
        {
            InitializeComponent();
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
