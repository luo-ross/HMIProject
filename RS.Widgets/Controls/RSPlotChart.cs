using RS.Commons.Extend;
using RS.Widgets.Extensions;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class RSPlotChart : WpfPlot
    {


        [Description("图标背景色")]
        public Color FigureBackground
        {
            get { return (Color)GetValue(FigureBackgroundProperty); }
            set { SetValue(FigureBackgroundProperty, value); }
        }

        public static readonly DependencyProperty FigureBackgroundProperty =
            DependencyProperty.Register("FigureBackground", typeof(Color), typeof(RSPlotChart), new PropertyMetadata(Colors.White));

        [Description("数据背景色")]
        public Color DataBackground
        {
            get { return (Color)GetValue(DataBackgroundProperty); }
            set { SetValue(DataBackgroundProperty, value); }
        }

        public static readonly DependencyProperty DataBackgroundProperty =
            DependencyProperty.Register("DataBackground", typeof(Color), typeof(RSPlotChart), new PropertyMetadata(Colors.White));






        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            // 检测 Background 属性的变化
            if (e.Property == DataBackgroundProperty)
            {
                this.Plot.DataBackground.Color = this.DataBackground.ToScottPlotColor();
            }
            else if (e.Property == FigureBackgroundProperty)
            {
                this.Plot.FigureBackground.Color = this.FigureBackground.ToScottPlotColor();
            }
            else if (e.Property == BackgroundProperty)
            {
                this.Plot.FigureBackground.Color = this.Background.ToScottPlotColor();
                this.Plot.FigureBackground.Color = this.Background.ToScottPlotColor();
            }
            else if (e.Property == BorderThicknessProperty)
            {
                //this.PlotChart.Plot.Axes 是控制4个边框的
                this.Plot.Axes.Right.FrameLineStyle.Width =this.BorderThickness.Right.ToFloat();
                this.Plot.Axes.Top.FrameLineStyle.Width = this.BorderThickness.Top.ToFloat();
                this.Plot.Axes.Left.FrameLineStyle.Width = this.BorderThickness.Left.ToFloat();
                this.Plot.Axes.Bottom.FrameLineStyle.Width = this.BorderThickness.Bottom.ToFloat();
            }
            else if (e.Property == BorderBrushProperty)
            {
                this.Plot.Axes.Left.FrameLineStyle.Color = this.BorderBrush.ToScottPlotColor();
                this.Plot.Axes.Right.FrameLineStyle.Color = this.BorderBrush.ToScottPlotColor();
                this.Plot.Axes.Bottom.FrameLineStyle.Color = this.BorderBrush.ToScottPlotColor();
                this.Plot.Axes.Top.FrameLineStyle.Color = this.BorderBrush.ToScottPlotColor();
            }
        }
    }
}
