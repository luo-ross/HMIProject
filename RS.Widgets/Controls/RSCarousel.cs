using Castle.Core.Logging;
using ScottPlot.Statistics;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SkiaSharp;
using RS.Widgets.Models;

namespace RS.Widgets.Controls
{
    public class RSCarousel : RSUserControl
    {
        private Canvas PART_Canvas;
        static RSCarousel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSCarousel), new FrameworkPropertyMetadata(typeof(RSCarousel)));
        }


        [Description("轮播数据集")]
        public ObservableCollection<CarouselSlider> CarouselSliderList
        {
            get { return (ObservableCollection<CarouselSlider>)GetValue(CarouselSliderListProperty); }
            set { SetValue(CarouselSliderListProperty, value); }
        }

        public static readonly DependencyProperty CarouselSliderListProperty =
            DependencyProperty.Register("CarouselSliderList", typeof(ObservableCollection<CarouselSlider>), typeof(RSCarousel), new PropertyMetadata(null, CarouselSliderListPropertyChanged));

        private static void CarouselSliderListPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsCarousel = d as RSCarousel;
            rsCarousel?.RefeshCarousel();
        }

        private void RefeshCarousel()
        {
            if (this.CarouselSliderList == null)
            {
                return;
            }
            if (this.PART_Canvas == null)
            {
                return;
            }
            this.PART_Canvas.Children.Clear();
            foreach (var item in CarouselSliderList)
            {
                RSCarouselSlider rsCarouselSlider = new RSCarouselSlider();
                rsCarouselSlider.Name = item.Name;
                rsCarouselSlider.Background = (Brush)new BrushConverter().ConvertFrom(item.Background);
                rsCarouselSlider.BlurRadius = 3;
                rsCarouselSlider.Caption = item.Caption;
                rsCarouselSlider.Description = item.Description;
                rsCarouselSlider.ImageSource = item.ImageSource;
                rsCarouselSlider.Location = item.Location;
                this.PART_Canvas.Children.Add(rsCarouselSlider);

                #region 需要动态计算待处理
                Canvas.SetLeft(rsCarouselSlider, 0);
                Canvas.SetTop(rsCarouselSlider, 0);
                Panel.SetZIndex(rsCarouselSlider, 14);
                rsCarouselSlider.Scale = 1;
                #endregion
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Canvas = this.GetTemplateChild(nameof(PART_Canvas)) as Canvas;
            this.RefeshCarousel();
        }
    }
}
