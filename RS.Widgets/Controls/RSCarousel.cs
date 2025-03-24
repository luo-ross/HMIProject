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
using System.Windows.Controls.Primitives;
using NPOI.SS.Formula.Functions;
using NPOI.POIFS.Properties;
using NPOI.Util;

namespace RS.Widgets.Controls
{

    /// <summary>
    ///  Fov算法，计算FieldOfView公式= 2 * arctan(尺寸 / (2 * 距离)) * (180 / π) 
    /// </summary>
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

            ////根据用户传递过来的数据需要处理一下
            //this.DataInit();

            //var middleIndex = this.CarouselSliderList.Count / 2;
            //this?.RefeshCarousel(middleIndex);
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


        }


        private void DataInit()
        {

        }

        /// <summary>
        /// 缩放后的轮廓图片高度差
        /// </summary>
        public int SliderScaleHeightDiff = 20;
        public double MaxScale = 1;
        public int MarginRight { get; set; }

        private void RefeshCarousel(int middleIndex)
        {
            if (this.CarouselSliderList == null)
            {
                return;
            }
            if (this.PART_Canvas == null)
            {
                return;
            }

            //获取容器的宽度和高度
            var canvasWidth = this.PART_Canvas.ActualWidth;
            var canvasHeight = this.PART_Canvas.ActualHeight;
            //获取Slider的长宽

            var viewCenterX = this.ActualWidth / 2;
            var viewCenterY = this.ActualHeight / 2;
            //第一步确定缩放函输y=-k|x|+b;

            //直线方程的K值
            double k = SliderScaleHeightDiff / (this.SliderWidth / 2);

            //获取屏幕最中间的控件左上角的坐标点，这个点恰好落在这条直线上

            // 斜率等于对边比邻边
            var x = -this.SliderWidth / 2;
            var y = this.SliderHeight / 2;
            //已知x,y和k 求出b
            var b = y + k * Math.Abs(x);

            //根据Y=0 计算出X值
            var xAxisValue = b / k;


            //第一步



            //第一步先把图显示出来
            int marginRight = 30;
            for (int i = 0; i < this.CarouselSliderList.Count; i++)
            {
                var item = this.CarouselSliderList[i];
                item.Index = i;
                item.Width = this.SliderWidth;
                item.Height = this.SliderHeight;
                item.CanvasTop = viewCenterY - this.SliderHeight / 2;

                item.CanvasLeft = i * item.Width;
                //设置模糊度
                if (item.Index == middleIndex)
                {
                    item.Blur = 0;
                }
                else
                {
                    item.Blur = 3;
                }

                //计算ZIndex层级
                if (item.Index < middleIndex)
                {
                    item.ZIndex = i - middleIndex;
                }
                else if (item.Index == middleIndex)
                {
                    item.ZIndex = i;
                }
                else
                {
                    item.ZIndex = middleIndex - i;
                }

                //计算中心点 通过左上角Canvas坐标，可以获取到控件的中心坐标
                item.CenterX = item.CanvasLeft + this.SliderWidth / 2;
                item.CenterY = item.CanvasTop + this.SliderHeight / 2;



                ////计算中心点到
                //marginLeftSum += 150;
            }

            //获取居中的Slider
            var middleSlider = this.CarouselSliderList[middleIndex];
            //计算居中的这个中心和视野中心的距离
            var transformX = -(middleSlider.CenterX - viewCenterX);

            //foreach (var item in this.CarouselSliderList)
            //{
            //    item.TransformX = item.TransformX + transformX;
            //}

            TransformGroup transformGroup = new TransformGroup();
            //item.ScaleTransform = new ScaleTransform()
            //{
            //    ScaleX = item.Scale,
            //    ScaleY = item.Scale
            //};
            var translateTransform = new TranslateTransform()
            {
                X = transformX,
                Y = 0
            };
            transformGroup.Children.Add(translateTransform);
            //transformGroup.Children.Add(item.ScaleTransform);
            this.PART_Canvas.RenderTransform = transformGroup;

            for (int i = 0; i < this.CarouselSliderList.Count; i++)
            {
                var item = this.CarouselSliderList[i];
                var transformCenter = translateTransform.Transform(new Point(item.CenterX, item.CenterY));


                //计算中心坐标点到屏幕中间圆点的距离 得到在X轴上的坐标点
                var axisXDistance = Math.Abs(viewCenterX - transformCenter.X);
                var scale = this.MaxScale - Math.Abs(axisXDistance / xAxisValue);
                //item.Scale = Math.Max(0, scale);
                //var scaleWidth = item.Scale * item.Width;
                //item.ScaleWidthDif = item.Width - scaleWidth;
            }

            ////通过计算每个Slider的宽度，得到整个Canvas的宽度
            //var totalWidth = this.CarouselSliderList.Sum(t => t.Width);
            ////计算出Canvas的中心点
            //var canvasCenterX = totalWidth / 2;
            //var canvasCenterY = this.PART_Canvas.ActualHeight / 2;

            ////获取默认居中的Slider
            //var middleSlider = this.CarouselSliderList[middleIndex];
            ////计算居中Slider的中心点
            //var middleSliderCenterX = middleSlider.CanvasLeft + middleSlider.Width / 2;
            //var middleSliderCenterY = middleSlider.CanvasTop + middleSlider.Height / 2;
            ////计算出Slider和视窗中心点的距离获取偏移位置
            //var transformXShould = middleSliderCenterX - viewCenterX;

            //foreach (var item in this.CarouselSliderList)
            //{
            //    item.TransformX = -transformXShould;
            //    //获取每个Slider的中心
            //    var sliderCenterX = item.CanvasLeft + middleSlider.Width / 2;
            //    var sliderCenterY = item.CanvasTop + middleSlider.Height / 2;
            //    //计算离屏幕中心的距离然后除以最大屏幕宽度的一半得到百分比
            //    var percent = Math.Abs(sliderCenterX - viewCenterX - transformXShould) / halfScreenWidth;
            //    //离屏幕中心越远缩放比例越大
            //    item.Scale = this.MaxScale - percent;
            //    //缩放后Slider与Slider之间距离发生了变化
            //    //需要动态补差
            //    //计算缩放后的宽度 和原始宽度之间变化值
            //    var itemScaleWidth = item.Width * item.Scale;
            //    var scaleWidthDiff = item.Width - itemScaleWidth;
            //    //距离屏幕中间左边的需要向右移动补偿
            //    //距离屏幕中间右边的需要向左移动补偿
            //    item.ScaleWidthDif = scaleWidthDiff;
            //}

            double marginBase = 160;
            double marginPercent = 0.2;
            var leftSliderList = this.CarouselSliderList.Where(t => t.Index < middleIndex).ToList();
            //foreach (var item in leftSliderList)
            //{
            //    item.TransformX = item.TransformX - marginBase;
            //}
            //var rightSliderList = this.CarouselSliderList.Where(t => t.Index > middleIndex).ToList();
            //foreach (var item in rightSliderList)
            //{
            //    item.TransformX = item.TransformX + marginBase; ;
            //}

            foreach (var item in this.CarouselSliderList)
            {
                if (item.RSCarouselSlider == null)
                {
                    RSCarouselSlider rsCarouselSlider = new RSCarouselSlider();
                    item.RSCarouselSlider = rsCarouselSlider;
                    this.PART_Canvas.Children.Add(item.RSCarouselSlider);
                }

                item.RSCarouselSlider.Name = item.Name;
                item.RSCarouselSlider.Background = (Brush)new BrushConverter().ConvertFrom(item.Background);
                item.RSCarouselSlider.Caption = item.Caption;
                item.RSCarouselSlider.Description = item.Description;
                item.RSCarouselSlider.ImageSource = item.ImageSource;
                item.RSCarouselSlider.Location = item.Location;
                item.RSCarouselSlider.DragDelta += RsCarouselSlider_DragDelta;
                item.RSCarouselSlider.Width = this.SliderWidth;
                item.RSCarouselSlider.Height = this.SliderHeight;
                item.RSCarouselSlider.Tag = item;
                item.RSCarouselSlider.BlurRadius = item.Blur;
                Canvas.SetLeft(item.RSCarouselSlider, item.CanvasLeft);
                Canvas.SetTop(item.RSCarouselSlider, item.CanvasTop);
                Panel.SetZIndex(item.RSCarouselSlider, item.ZIndex);
                item.RSCarouselSlider.RenderTransformOrigin = new Point(0.5, 0.5);
                if (item.RSCarouselSlider.RenderTransform == Transform.Identity)
                {
                    transformGroup = new TransformGroup();
                    item.ScaleTransform = new ScaleTransform()
                    {
                        ScaleX = item.Scale,
                        ScaleY = item.Scale
                    };
                    //item.TranslateTransform = new TranslateTransform()
                    //{
                    //    X = item.TransformX,
                    //    Y = item.TransformX
                    //};
                    //transformGroup.Children.Add(item.TranslateTransform);
                    transformGroup.Children.Add(item.ScaleTransform);
                    item.RSCarouselSlider.RenderTransform = transformGroup;
                }

                item.RSCarouselSlider.RenderTransformOrigin = new Point(0.5, 0.5);

            }
        }

        private void RsCarouselSlider_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var rsCarouselSlider = sender as RSCarouselSlider;
            var transformGroup = this.PART_Canvas.RenderTransform as TransformGroup;

            if (transformGroup != null)
            {
                var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();
                if (translateTransform != null)
                {
                    translateTransform.X = translateTransform.X + e.HorizontalChange;
                }
            }
            //foreach (var item in this.CarouselSliderList)
            //{
            //    item.TransformX = item.TransformX + e.HorizontalChange;
            //    var transformGroup = item.RSCarouselSlider.RenderTransform as TransformGroup;
            //    if (transformGroup != null)
            //    {
            //        var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();
            //        if (translateTransform != null)
            //        {
            //            translateTransform.X = item.TransformX;
            //        }
            //        var scaleTransform = transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();
            //        if (scaleTransform != null)
            //        {
            //            scaleTransform.ScaleX = item.Scale;
            //            scaleTransform.ScaleX = item.Scale;
            //        }
            //    }
            //}

            ////要动态计算
            ////大于0向右移动
            //if (e.HorizontalChange > 0)
            //{
            //    this.MiddleIndex++;
            //    this.MiddleIndex = Math.Min(this.CarouselSliderList.Count - 1, this.MiddleIndex);
            //}
            //else
            //{
            //    this.MiddleIndex--;
            //    this.MiddleIndex = Math.Max(0, this.MiddleIndex);
            //}
            //RefeshCarousel(this.MiddleIndex);
        }






        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Canvas = this.GetTemplateChild(nameof(PART_Canvas)) as Canvas;

        }
    }
}
