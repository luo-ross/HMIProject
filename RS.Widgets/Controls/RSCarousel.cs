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
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Media.Media3D;

namespace RS.Widgets.Controls
{
    public class RSCarousel : RSUserControl
    {
        private Canvas PART_Canvas;
        static RSCarousel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSCarousel), new FrameworkPropertyMetadata(typeof(RSCarousel)));
        }

        public RSCarousel()
        {
            this.Loaded += RSCarousel_Loaded;
        }

        private void RSCarousel_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefeshCarousel();
        }



        [Description("轮播图宽度")]
        public double SliderWidth
        {
            get { return (double)GetValue(SliderWidthProperty); }
            set { SetValue(SliderWidthProperty, value); }
        }

        public static readonly DependencyProperty SliderWidthProperty =
            DependencyProperty.Register("SliderWidth", typeof(double), typeof(RSCarousel), new PropertyMetadata(300D));




        [Description("轮播图高度")]
        public double SliderHeight
        {
            get { return (double)GetValue(SliderHeightProperty); }
            set { SetValue(SliderHeightProperty, value); }
        }

        public static readonly DependencyProperty SliderHeightProperty =
            DependencyProperty.Register("SliderHeight", typeof(double), typeof(RSCarousel), new PropertyMetadata(400D));





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

        private List<RSCarouselSlider> DataList1 = new List<RSCarouselSlider>();
        private List<RSCarouselSlider> DataList2 = new List<RSCarouselSlider>();

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


            //获取容器的宽度和高度
            var canvasWidth = this.PART_Canvas.ActualWidth;
            var canvasHeight = this.PART_Canvas.ActualHeight;
            //获取Slider的长宽

            for (int i = 0; i < CarouselSliderList.Count; i++)
            {
                var item = CarouselSliderList[i];
                RSCarouselSlider rsCarouselSlider = new RSCarouselSlider();
                rsCarouselSlider.Name = item.Name;
                rsCarouselSlider.Background = (Brush)new BrushConverter().ConvertFrom(item.Background);
                rsCarouselSlider.BlurRadius = 5;
                rsCarouselSlider.Caption = item.Caption;
                rsCarouselSlider.Description = item.Description;
                rsCarouselSlider.ImageSource = item.ImageSource;
                rsCarouselSlider.Location = item.Location;
                rsCarouselSlider.DragDelta += RsCarouselSlider_DragDelta;
                rsCarouselSlider.Width = this.SliderWidth;
                rsCarouselSlider.Height = this.SliderHeight;
                DataList1.Add(rsCarouselSlider);
                this.PART_Canvas.Children.Add(rsCarouselSlider);

                if (i>0)
                {
                    rsCarouselSlider = new RSCarouselSlider();
                    rsCarouselSlider.Name = item.Name;
                    rsCarouselSlider.Background = (Brush)new BrushConverter().ConvertFrom(item.Background);
                    rsCarouselSlider.BlurRadius = 5;
                    rsCarouselSlider.Caption = item.Caption;
                    rsCarouselSlider.Description = item.Description;
                    rsCarouselSlider.ImageSource = item.ImageSource;
                    rsCarouselSlider.Location = item.Location;
                    rsCarouselSlider.DragDelta += RsCarouselSlider_DragDelta;
                    rsCarouselSlider.Width = this.SliderWidth;
                    rsCarouselSlider.Height = this.SliderHeight;
                    DataList2.Add(rsCarouselSlider);
                    this.PART_Canvas.Children.Add(rsCarouselSlider);
                }
            }
           


            //直接设置画布的总长度
            double drawMapWidth = 4000D;

            double canvasLeft = 0;
            double marginLeft = 20;
            double maxScale = 1;
            double minScale = 0.2;

            double transformXTotal = 0;
            var halfDrawMapWidth = drawMapWidth / 2;
            for (int i = 0; i < DataList1.Count; i++)
            {
                var item = DataList1[i];

                //如果是最中间这个不进行模糊
                if (i == 0)
                {
                    item.BlurRadius = 0;
                }

                //需要重新计算每一个Slider的默认长宽
                var sliderWidth = item.Width;
                var sliderHeight = item.Height;
                if (sliderHeight > canvasHeight)
                {
                    item.Height = canvasHeight;
                }
                if (sliderWidth > canvasWidth)
                {
                    item.Width = canvasWidth;
                }

                //canvasTop数值一致
                var canvasTop = canvasHeight / 2 - item.Height / 2;

                //canvasLeft动态计算
                if (i == 0)
                {
                    canvasLeft = canvasWidth / 2 - item.Width / 2;
                }
                else
                {
                    canvasLeft = canvasLeft + item.Width;
                }

                Canvas.SetLeft(item, canvasLeft);
                Canvas.SetTop(item, canvasTop);
                Panel.SetZIndex(item, DataList1.Count - i);
                //获取Slider的中心点
                var percent = (i * this.SliderWidth) / halfDrawMapWidth;
                var scale = maxScale - percent;
                //缩放最小值0.2
                scale = Math.Max(minScale, scale);

                item.RenderTransformOrigin = new Point(0.5, 0.5);

                TransformGroup transformGroup = new TransformGroup();
                ScaleTransform scaleTransform = new ScaleTransform()
                {
                    ScaleX = scale,
                    ScaleY = scale
                };
                transformGroup.Children.Add(scaleTransform);

                var widthScale = item.Width * scale;
                var widthDif = item.Width - widthScale;

                var transformX = -widthDif / 2 - transformXTotal;

                if (i == 1)
                {
                    transformX = transformX + marginLeft;
                }
                else if (i > 1)
                {
                    transformX = transformX - marginLeft;
                }

                TranslateTransform translateTransform = new TranslateTransform()
                {
                    X = transformX,
                };
                transformGroup.Children.Add(translateTransform);
                item.RenderTransform = transformGroup;

                transformXTotal += widthDif;
            }

            transformXTotal = 0;

            for (int i = 0; i < DataList2.Count; i++)
            {
                var item = DataList2[i];

                //需要重新计算每一个Slider的默认长宽
                if (item.Height > canvasHeight)
                {
                    item.Height = canvasHeight;
                }
                if (item.Width > canvasWidth)
                {
                    item.Width = canvasWidth;
                }

                //canvasTop数值一致
                var canvasTop = canvasHeight / 2 - item.Height / 2;

                //canvasLeft动态计算
                if (i == 0)
                {
                    canvasLeft = canvasWidth / 2 - item.Width / 2 - item.Width;
                }
                else
                {
                    canvasLeft = canvasLeft - item.Width;
                }

                Canvas.SetLeft(item, canvasLeft);
                Canvas.SetTop(item, canvasTop);
                Panel.SetZIndex(item, DataList2.Count - i - 1);
                //获取Slider的中心点
                var percent = ((i + 1) * this.SliderWidth) / halfDrawMapWidth;
                var scale = maxScale - percent;
                //缩放最小值0.2
                scale = Math.Max(minScale, scale);

                item.RenderTransformOrigin = new Point(0.5, 0.5);

                TransformGroup transformGroup = new TransformGroup();
                ScaleTransform scaleTransform = new ScaleTransform()
                {
                    ScaleX = scale,
                    ScaleY = scale
                };
                transformGroup.Children.Add(scaleTransform);

                var widthScale = item.Width * scale;
                var widthDif = item.Width - widthScale;

                var transformX = widthDif / 2 + transformXTotal;

                if (i == 0)
                {
                    transformX = transformX - marginLeft;
                }
                else
                {
                    transformX = transformX + marginLeft;
                }
                TranslateTransform translateTransform = new TranslateTransform()
                {
                    X = transformX,
                };
                transformGroup.Children.Add(translateTransform);
                item.RenderTransform = transformGroup;
                transformXTotal += widthDif;
            }

            RereshCarouselSliderPosition();
        }


        private void RereshCarouselSliderPosition()
        {


        }

        private void RsCarouselSlider_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var sf = 1;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Canvas = this.GetTemplateChild(nameof(PART_Canvas)) as Canvas;

        }
    }
}
